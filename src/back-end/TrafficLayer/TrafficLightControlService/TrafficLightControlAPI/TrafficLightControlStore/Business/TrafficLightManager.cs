using System;
using System.Collections.Concurrent;
using TrafficLightControlStore.Publishers;
using TrafficLightControlStore.Publishers.Light;
using TrafficLightControlStore.Publishers.Logs;
using TrafficMessages.Light;

namespace TrafficLightControlStore.Business;

 public class TrafficLightManager : ITrafficLightManager
    {
        private readonly ITrafficLightUpdatePublisher _updatePublisher;
        private readonly ITrafficLogPublisher _logPublisher;
        private readonly ILogger<TrafficLightManager> _logger;

        // light_id -> (state, updated_at, cancellation_token)
        private static readonly ConcurrentDictionary<Guid, (string State, DateTime UpdatedAt, CancellationTokenSource Cts)> _lights = new();

        public TrafficLightManager(
            ITrafficLightUpdatePublisher updatePublisher,
            ITrafficLogPublisher logPublisher,
            ILogger<TrafficLightManager> logger)
        {
            _updatePublisher = updatePublisher;
            _logPublisher = logPublisher;
            _logger = logger;
        }

        public async Task ApplyControlAsync(TrafficLightControl control)
        {
            if (!Guid.TryParse(control.IntersectionId, out var lightId))
            {
                _logger.LogError("Invalid light ID: {IntersectionId}", control.IntersectionId);
                return;
            }

            await SetLightStateAsync(lightId, control.ControlPattern, control.DurationSeconds, control.TriggeredBy);
        }

        public async Task OverrideLightAsync(Guid lightId, string state, int durationSeconds, string triggeredBy)
        {
            await SetLightStateAsync(lightId, state, durationSeconds, triggeredBy);
        }

        public (string State, DateTime UpdatedAt)? GetLightState(Guid lightId)
        {
            if (_lights.TryGetValue(lightId, out var info))
            {
                return (info.State, info.UpdatedAt);
            }
            return null;
        }

        private async Task SetLightStateAsync(Guid lightId, string state, int durationSeconds, string triggeredBy)
        {
            if (_lights.TryGetValue(lightId, out var existing))
            {
                existing.Cts?.Cancel();
            }

            var cts = new CancellationTokenSource();
            _lights[lightId] = (state, DateTime.UtcNow, cts);

            // Publish light update
            await _updatePublisher.PublishUpdateAsync(lightId.ToString(), state);

            // Log it
            await _logPublisher.PublishAuditLogAsync("TrafficLightControlService",
                $"Light {lightId} set to {state} for {durationSeconds}s by {triggeredBy}");

            _logger.LogInformation("[LIGHT MANAGER] Light {LightId} set to {State} for {Duration}s by {TriggeredBy}",
                lightId, state, durationSeconds, triggeredBy);

            // Auto-revert after duration
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(durationSeconds * 1000, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        _lights[lightId] = ("IDLE", DateTime.UtcNow, null);
                        await _updatePublisher.PublishUpdateAsync(lightId.ToString(), "IDLE");
                        await _logPublisher.PublishAuditLogAsync("TrafficLightControlService",
                            $"Light {lightId} reverted to IDLE after {durationSeconds}s");
                    }
                }
                catch (TaskCanceledException) { }
            });
        }
    }
