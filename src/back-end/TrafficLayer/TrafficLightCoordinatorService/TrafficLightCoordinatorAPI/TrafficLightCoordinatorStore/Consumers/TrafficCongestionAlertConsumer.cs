using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using TrafficLightCoordinatorStore.Consumers;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficMessages.Analytics;        // for PatternBuilder.For(...)

namespace TrafficLightCoordinatorStore.Consumers
{
    public class TrafficCongestionAlertConsumer : IConsumer<TrafficCongestionAlert>
    {
        private readonly ILightUpdatePublisher _publisher;
        private readonly ILogger<TrafficCongestionAlertConsumer> _logger;

        private readonly string _queueName;
        private readonly string _exchangeName;
        private readonly string _routingKeyPattern;

        public TrafficCongestionAlertConsumer(
            ILightUpdatePublisher publisher,
            ILogger<TrafficCongestionAlertConsumer> logger,
            IConfiguration configuration)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var section = configuration.GetSection("RabbitMQ");
            _queueName = section["Queue:TrafficCoordinationQueue"]                      ?? "traffic.light.coordination";
            _exchangeName = section["Exchange:TrafficAnalyticsCongestionAlert"]         ?? "traffic.analytics.congestion_alert";
            _routingKeyPattern = section["RoutingKey:TrafficAnalyticsCongestionKey"]    ?? "*";
        }

        public async Task Consume(ConsumeContext<TrafficCongestionAlert> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "[CONGESTION] received on '{Exchange}' key '{Key}' -> intersection {Id}, level={Level}, severity={Severity}, ts={Ts}, desc={Desc}",
                _exchangeName, _routingKeyPattern, msg.IntersectionId, msg.CongestionLevel, msg.Severity, msg.Timestamp, msg.Description);

            // very simple mapping: High => apply a stronger green bias (reuse “public_transport” pattern),
            // otherwise keep default cycle.
            var active = string.Equals(msg.Severity, "High", StringComparison.OrdinalIgnoreCase)
                         || msg.CongestionLevel >= 0.75;

            var pattern = PatternBuilder.For("public_transport", active);

            await _publisher.PublishAsync(msg.IntersectionId, pattern, context.CancellationToken);

            _logger.LogInformation("[CONGESTION] applied -> {Pattern}", pattern);
        }
    }
}
