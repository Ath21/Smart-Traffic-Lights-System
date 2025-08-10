using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntersectionControlStore.Models;
using MassTransit;
using SensorMessages.Data;
using TrafficMessages.Priority;
using IntersectionControlStore.Publishers.PriorityPub;

namespace IntersectionControlStore.Business
{
    public class PriorityManager : IPriorityManager
    {
        private readonly IPriorityPublisher _priorityPublisher;
        private readonly Dictionary<Guid, IntersectionPriorityStatus> _priorityStates = new();

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
            if (_priorityStates.TryGetValue(intersectionId, out var existingStatus))
            {
                existingStatus.OverrideCancellationTokenSource?.Cancel(); // cancel previous override if any
            }
            else
            {
                existingStatus = new IntersectionPriorityStatus { IntersectionId = intersectionId };
                _priorityStates[intersectionId] = existingStatus;
            }

            existingStatus.PriorityEmergencyVehicle = overrideStatus.PriorityEmergencyVehicle;
            existingStatus.PriorityPublicTransport = overrideStatus.PriorityPublicTransport;
            existingStatus.PriorityPedestrian = overrideStatus.PriorityPedestrian;
            existingStatus.PriorityCyclist = overrideStatus.PriorityCyclist;
            existingStatus.UpdatedAt = DateTime.UtcNow;

            var cts = new CancellationTokenSource();
            existingStatus.OverrideCancellationTokenSource = cts;

            // Publish updated priorities immediately via publisher
            await _priorityPublisher.PublishPrioritiesAsync(existingStatus);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(durationSeconds * 1000, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        // After override duration expires, clear override and reset to default (e.g., no priorities)
                        existingStatus.PriorityEmergencyVehicle = false;
                        existingStatus.PriorityPublicTransport = false;
                        existingStatus.PriorityPedestrian = false;
                        existingStatus.PriorityCyclist = false;
                        existingStatus.UpdatedAt = DateTime.UtcNow;
                        existingStatus.OverrideCancellationTokenSource = null;

                        await _priorityPublisher.PublishPrioritiesAsync(existingStatus);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Override was canceled by a new override, do nothing
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

                // Add more sensor types as needed

                default:
                    break;
            }
        }

        private async Task UpdatePriority(Guid intersectionId,
            bool? priorityEmergencyVehicle = null,
            bool? priorityPublicTransport = null,
            bool? priorityPedestrian = null,
            bool? priorityCyclist = null)
        {
            if (!_priorityStates.TryGetValue(intersectionId, out var status))
            {
                status = new IntersectionPriorityStatus
                {
                    IntersectionId = intersectionId,
                    UpdatedAt = DateTime.UtcNow
                };
                _priorityStates[intersectionId] = status;
            }

            // Skip update if overridden manually
            if (status.OverrideCancellationTokenSource != null)
                return;

            if (priorityEmergencyVehicle.HasValue) status.PriorityEmergencyVehicle = priorityEmergencyVehicle.Value;
            if (priorityPublicTransport.HasValue) status.PriorityPublicTransport = priorityPublicTransport.Value;
            if (priorityPedestrian.HasValue) status.PriorityPedestrian = priorityPedestrian.Value;
            if (priorityCyclist.HasValue) status.PriorityCyclist = priorityCyclist.Value;
            status.UpdatedAt = DateTime.UtcNow;

            await _priorityPublisher.PublishPrioritiesAsync(status);
        }
    }
}
