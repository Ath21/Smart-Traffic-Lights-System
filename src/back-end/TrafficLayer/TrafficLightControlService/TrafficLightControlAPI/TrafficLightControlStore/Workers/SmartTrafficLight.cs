using System;
using System.Collections.Concurrent;
using TrafficMessages.Light;

namespace TrafficLightControlStore.Workers;

 public class TrafficLightWorker : ITrafficLigh
    {
        private readonly ITrafficLightUpdatePublisher _updatePublisher;
        private readonly ILogger<TrafficLightWorker> _logger;
        private static readonly ConcurrentDictionary<string, (string Pattern, DateTime UpdatedAt, CancellationTokenSource Cts)> _lights = new();

        public TrafficLightWorker(ITrafficLightUpdatePublisher updatePublisher, ILogger<TrafficLightWorker> logger)
        {
            _updatePublisher = updatePublisher;
            _logger = logger;
        }

        public async Task ApplyControlAsync(TrafficLightControl control)
        {
            if (_lights.TryGetValue(control.IntersectionId, out var existing))
            {
                existing.Cts?.Cancel();
            }

            var cts = new CancellationTokenSource();
            _lights[control.IntersectionId] = (control.ControlPattern, DateTime.UtcNow, cts);

            // Immediately publish light update
            await _updatePublisher.PublishUpdateAsync(control.IntersectionId, control.ControlPattern);

            // Simulate running the pattern for its duration
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(control.DurationSeconds * 1000, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        _lights[control.IntersectionId] = ("IDLE", DateTime.UtcNow, null);
                        await _updatePublisher.PublishUpdateAsync(control.IntersectionId, "IDLE");
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogDebug("Control pattern for {IntersectionId} canceled early", control.IntersectionId);
                }
            });

            _logger.LogInformation("[LIGHT WORKER] Applied pattern {Pattern} to {IntersectionId}", control.ControlPattern, control.IntersectionId);
        }
    }