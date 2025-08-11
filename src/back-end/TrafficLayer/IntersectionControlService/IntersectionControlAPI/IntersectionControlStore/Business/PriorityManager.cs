using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using IntersectionControlStore.Models;
using TrafficMessages.Priority;
using IntersectionControlStore.Publishers.PriorityPub;
using SensorMessages.Data;

namespace IntersectionControlStore.Business
{
    public class PriorityManager : IPriorityManager
    {
        private readonly IPriorityPublisher _priorityPublisher;
        
        // Use ConcurrentDictionary for thread-safe access in async environment
        private readonly ConcurrentDictionary<Guid, IntersectionPriorityStatus> _priorityStates = new();

        public PriorityManager(IPriorityPublisher priorityPublisher)
        {
            _priorityPublisher = priorityPublisher;
        }

        public IntersectionPriorityStatus? GetPriorityStatus(Guid intersectionId)
        {
            _priorityStates.TryGetValue(intersectionId, out var status);
            return status;
        }

        public async Task OverridePriorityAsync(Guid intersectionId, IntersectionPriorityStatus overrideStatus, int durationSeconds)
        {
            var status = _priorityStates.GetOrAdd(intersectionId, id => new IntersectionPriorityStatus { IntersectionId = id });

            // Cancel any existing override cancellation token
            status.OverrideCancellationTokenSource?.Cancel();

            // Apply override values
            status.PriorityEmergencyVehicle = overrideStatus.PriorityEmergencyVehicle;
            status.PriorityPublicTransport = overrideStatus.PriorityPublicTransport;
            status.PriorityPedestrian = overrideStatus.PriorityPedestrian;
            status.PriorityCyclist = overrideStatus.PriorityCyclist;
            status.UpdatedAt = DateTime.UtcNow;

            var cts = new CancellationTokenSource();
            status.OverrideCancellationTokenSource = cts;

            // Publish immediately
            await _priorityPublisher.PublishPrioritiesAsync(status);

            // Run the override expiration in background without blocking
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(durationSeconds), cts.Token);

                    if (!cts.Token.IsCancellationRequested)
                    {
                        // Clear override - reset all priorities
                        status.PriorityEmergencyVehicle = false;
                        status.PriorityPublicTransport = false;
                        status.PriorityPedestrian = false;
                        status.PriorityCyclist = false;
                        status.UpdatedAt = DateTime.UtcNow;
                        status.OverrideCancellationTokenSource = null;

                        await _priorityPublisher.PublishPrioritiesAsync(status);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore cancellation, as a new override was set
                }
            });
        }

        public async Task ProcessSensorMessageAsync(object sensorMessage)
        {
            switch (sensorMessage)
            {
                case EmergencyVehicleDetectionMessage evm:
                    await UpdatePriority(evm.IntersectionId, priorityEmergencyVehicle: true);
                    break;

                case PedestrianDetectionMessage pdm:
                    await UpdatePriority(pdm.IntersectionId, priorityPedestrian: pdm.PedestrianCount > 0);
                    break;

                case PublicTransportDetectionMessage ptm:
                    await UpdatePriority(ptm.IntersectionId, priorityPublicTransport: ptm.PassengerCount > 0);
                    break;

                case CyclistDetectionMessage cdm:
                    await UpdatePriority(cdm.IntersectionId, priorityCyclist: cdm.CyclistCount > 0);
                    break;

                // Extend with other sensor message types as needed

                default:
                    // Unknown sensor message type - ignore
                    break;
            }
        }

        private async Task UpdatePriority(Guid intersectionId,
            bool? priorityEmergencyVehicle = null,
            bool? priorityPublicTransport = null,
            bool? priorityPedestrian = null,
            bool? priorityCyclist = null)
        {
            var status = _priorityStates.GetOrAdd(intersectionId, id => new IntersectionPriorityStatus
            {
                IntersectionId = id,
                UpdatedAt = DateTime.UtcNow
            });

            // If override is active, do not update from sensors
            if (status.OverrideCancellationTokenSource != null)
                return;

            bool updated = false;

            if (priorityEmergencyVehicle.HasValue && status.PriorityEmergencyVehicle != priorityEmergencyVehicle.Value)
            {
                status.PriorityEmergencyVehicle = priorityEmergencyVehicle.Value;
                updated = true;
            }
            if (priorityPublicTransport.HasValue && status.PriorityPublicTransport != priorityPublicTransport.Value)
            {
                status.PriorityPublicTransport = priorityPublicTransport.Value;
                updated = true;
            }
            if (priorityPedestrian.HasValue && status.PriorityPedestrian != priorityPedestrian.Value)
            {
                status.PriorityPedestrian = priorityPedestrian.Value;
                updated = true;
            }
            if (priorityCyclist.HasValue && status.PriorityCyclist != priorityCyclist.Value)
            {
                status.PriorityCyclist = priorityCyclist.Value;
                updated = true;
            }

            if (updated)
            {
                status.UpdatedAt = DateTime.UtcNow;
                await _priorityPublisher.PublishPrioritiesAsync(status);
            }
        }
    }
}
