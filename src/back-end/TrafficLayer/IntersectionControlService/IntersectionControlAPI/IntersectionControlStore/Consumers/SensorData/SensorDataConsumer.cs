using System.Threading.Tasks;
using MassTransit;
using IntersectionControlStore.Business;  // For IPriorityManager
using SensorMessages.Data;               // For sensor message types
using Microsoft.Extensions.Logging;

namespace IntersectionControlStore.Consumers
{
    public class SensorDataConsumer :
        IConsumer<VehicleCountMessage>,
        IConsumer<EmergencyVehicleDetectionMessage>,
        IConsumer<PedestrianDetectionMessage>,
        IConsumer<PublicTransportDetectionMessage>,
        IConsumer<CyclistDetectionMessage>,
        IConsumer<IncidentDetectionMessage>
    // Add other sensor message types here
    {
        private readonly IPriorityManager _priorityManager;
        private readonly ILogger<SensorDataConsumer> _logger;

        public SensorDataConsumer(IPriorityManager priorityManager, ILogger<SensorDataConsumer> logger)
        {
            _priorityManager = priorityManager;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<VehicleCountMessage> context)
        {
            _logger.LogInformation("Received VehicleCountMessage for intersection {IntersectionId}", context.Message.IntersectionId);
            await _priorityManager.ProcessSensorMessageAsync(context.Message);
        }

        public async Task Consume(ConsumeContext<EmergencyVehicleDetectionMessage> context)
        {
            _logger.LogInformation("Received EmergencyVehicleDetectionMessage for intersection {IntersectionId}", context.Message.IntersectionId);
            await _priorityManager.ProcessSensorMessageAsync(context.Message);
        }

        public async Task Consume(ConsumeContext<PedestrianDetectionMessage> context)
        {
            _logger.LogInformation("Received PedestrianDetectionMessage for intersection {IntersectionId}", context.Message.IntersectionId);
            await _priorityManager.ProcessSensorMessageAsync(context.Message);
        }

        public async Task Consume(ConsumeContext<PublicTransportDetectionMessage> context)
        {
            _logger.LogInformation("Received PublicTransportDetectionMessage for intersection {IntersectionId}", context.Message.IntersectionId);
            await _priorityManager.ProcessSensorMessageAsync(context.Message);
        }

        public async Task Consume(ConsumeContext<CyclistDetectionMessage> context)
        {
            _logger.LogInformation("Received CyclistDetectionMessage for intersection {IntersectionId}", context.Message.IntersectionId);
            await _priorityManager.ProcessSensorMessageAsync(context.Message);
        }
        
        public async Task Consume(ConsumeContext<IncidentDetectionMessage> context)
        {
            _logger.LogInformation("Received IncidentDetectionMessage for intersection {IntersectionId}", context.Message.IntersectionId);
            await _priorityManager.ProcessSensorMessageAsync(context.Message);
        }
    }
}
