using System;
using System.Threading.Tasks;
using IntersectionControlStore.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages.Priority;

namespace IntersectionControlStore.Publishers.PriorityPub
{
    public class PriorityPublisher : IPriorityPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<PriorityPublisher> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _trafficControlExchange;
        private readonly string _priorityEmergencyVehicleRoutingKeyBase;
        private readonly string _priorityPublicTransportRoutingKeyBase;
        private readonly string _priorityPedestrianRoutingKeyBase;
        private readonly string _priorityCyclistRoutingKeyBase;

        public PriorityPublisher(
            IBus bus,
            ILogger<PriorityPublisher> logger,
            IConfiguration configuration)
        {
            _bus = bus;
            _logger = logger;
            _configuration = configuration;

            _trafficControlExchange = _configuration["RabbitMQ:Exchange:TrafficControlExchange"] ?? "traffic.control.exchange";

            // routing keys have a trailing .* pattern, we'll trim and append intersectionId dynamically
            _priorityEmergencyVehicleRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:PriorityEmergencyVehicle"] ?? "traffic.intersection_control.priority.emergency_vehicle.*";
            _priorityPublicTransportRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:PriorityPublicTransport"] ?? "traffic.intersection_control.priority.public_transport.*";
            _priorityPedestrianRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:PriorityPedestrian"] ?? "traffic.intersection_control.priority.pedestrian.*";
            _priorityCyclistRoutingKeyBase = _configuration["RabbitMQ:RoutingKey:PriorityCyclist"] ?? "traffic.intersection_control.priority.cyclist.*";
        }

        public Task PublishPriorityEmergencyVehicleAsync(string intersectionId, bool priority, DateTime updatedAt)
        {
            var routingKey = BuildRoutingKey(_priorityEmergencyVehicleRoutingKeyBase, intersectionId);
            var message = new PriorityEmergencyVehicle(intersectionId, priority, updatedAt);
            return PublishAsync(message, routingKey, "Emergency Vehicle Priority");
        }

        public Task PublishPriorityPublicTransportAsync(string intersectionId, bool priority, DateTime updatedAt)
        {
            var routingKey = BuildRoutingKey(_priorityPublicTransportRoutingKeyBase, intersectionId);
            var message = new PriorityPublicTransport(intersectionId, priority, updatedAt);
            return PublishAsync(message, routingKey, "Public Transport Priority");
        }

        public Task PublishPriorityPedestrianAsync(string intersectionId, bool priority, DateTime updatedAt)
        {
            var routingKey = BuildRoutingKey(_priorityPedestrianRoutingKeyBase, intersectionId);
            var message = new PriorityPedestrian(intersectionId, priority, updatedAt);
            return PublishAsync(message, routingKey, "Pedestrian Priority");
        }

        public Task PublishPriorityCyclistAsync(string intersectionId, bool priority, DateTime updatedAt)
        {
            var routingKey = BuildRoutingKey(_priorityCyclistRoutingKeyBase, intersectionId);
            var message = new PriorityCyclist(intersectionId, priority, updatedAt);
            return PublishAsync(message, routingKey, "Cyclist Priority");
        }

        public async Task PublishPrioritiesAsync(IntersectionPriorityStatus status)
        {
            var intersectionId = status.IntersectionId.ToString();

            await PublishPriorityEmergencyVehicleAsync(intersectionId, status.PriorityEmergencyVehicle, status.UpdatedAt);
            await PublishPriorityPublicTransportAsync(intersectionId, status.PriorityPublicTransport, status.UpdatedAt);
            await PublishPriorityPedestrianAsync(intersectionId, status.PriorityPedestrian, status.UpdatedAt);
            await PublishPriorityCyclistAsync(intersectionId, status.PriorityCyclist, status.UpdatedAt);
        }


        private string BuildRoutingKey(string routingKeyBase, string intersectionId)
        {
            var baseKey = routingKeyBase.TrimEnd('*').TrimEnd('.');
            return $"{baseKey}.{intersectionId}";
        }

        private async Task PublishAsync<T>(T message, string routingKey, string priorityType) where T : class
        {
            _logger.LogInformation("[PRIORITY] Publishing {PriorityType} message to '{RoutingKey}' on exchange '{Exchange}'", priorityType, routingKey, _trafficControlExchange);

            var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_trafficControlExchange}"));
            await sendEndpoint.Send(message, context =>
            {
                context.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[PRIORITY] Published {PriorityType} message for intersection", priorityType);
        }
    }
}
