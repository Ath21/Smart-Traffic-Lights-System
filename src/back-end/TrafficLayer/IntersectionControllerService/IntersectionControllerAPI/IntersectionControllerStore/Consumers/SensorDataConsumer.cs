using IntersectionControllerStore.Business.Priority;
using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace IntersectionControlStore.Consumers;

public class SensorDataConsumer :
    IConsumer<VehicleCountMessage>,
    IConsumer<EmergencyVehicleMessage>,
    IConsumer<PedestrianDetectionMessage>,
    IConsumer<PublicTransportMessage>,
    IConsumer<CyclistDetectionMessage>,
    IConsumer<IncidentDetectionMessage>
{
    private readonly IPriorityManager _priorityManager;
    private readonly ILogger<SensorDataConsumer> _logger;

    private const string ServiceTag = "[" + nameof(SensorDataConsumer) + "]";

    public SensorDataConsumer(IPriorityManager priorityManager, ILogger<SensorDataConsumer> logger)
    {
        _priorityManager = priorityManager;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<VehicleCountMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("{Tag} VehicleCountMessage received: Intersection={IntersectionId}, Count={Count}, AvgSpeed={AvgSpeed}",
            ServiceTag, msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed);

        await _priorityManager.ProcessVehicleCountAsync(msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed, msg.Timestamp);
    }

    public async Task Consume(ConsumeContext<EmergencyVehicleMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("{Tag} EmergencyVehicleMessage received: Intersection={IntersectionId}, DetectionId={DetectionId}, Detected={Detected}",
            ServiceTag, msg.IntersectionId, msg.DetectionId, msg.Detected);

        await _priorityManager.ProcessEmergencyVehicleAsync(msg.IntersectionId, msg.DetectionId, msg.Detected, msg.Timestamp);
    }

    public async Task Consume(ConsumeContext<PedestrianDetectionMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("{Tag} PedestrianDetectionMessage received: Intersection={IntersectionId}, DetectionId={DetectionId}, Count={Count}",
            ServiceTag, msg.IntersectionId, msg.DetectionId, msg.Count);

        await _priorityManager.ProcessPedestrianAsync(msg.IntersectionId, msg.DetectionId, msg.Count, msg.Timestamp);
    }

    public async Task Consume(ConsumeContext<PublicTransportMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("{Tag} PublicTransportMessage received: Intersection={IntersectionId}, DetectionId={DetectionId}, Route={RouteId}",
            ServiceTag, msg.IntersectionId, msg.DetectionId, msg.RouteId);

        await _priorityManager.ProcessPublicTransportAsync(msg.IntersectionId, msg.DetectionId, msg.RouteId, msg.Timestamp);
    }

    public async Task Consume(ConsumeContext<CyclistDetectionMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("{Tag} CyclistDetectionMessage received: Intersection={IntersectionId}, DetectionId={DetectionId}, Count={Count}",
            ServiceTag, msg.IntersectionId, msg.DetectionId, msg.Count);

        await _priorityManager.ProcessCyclistAsync(msg.IntersectionId, msg.DetectionId, msg.Count, msg.Timestamp);
    }

    public async Task Consume(ConsumeContext<IncidentDetectionMessage> context)
    {
        var msg = context.Message;
        _logger.LogWarning("{Tag} IncidentDetectionMessage received: Intersection={IntersectionId}, DetectionId={DetectionId}, Description={Description}",
            ServiceTag, msg.IntersectionId, msg.DetectionId, msg.Description);

        await _priorityManager.ProcessIncidentAsync(msg.IntersectionId, msg.DetectionId, msg.Description, msg.Timestamp);
    }
}
