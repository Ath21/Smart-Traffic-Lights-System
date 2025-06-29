using System;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public class DailySummaryAggregator : BackgroundService
{
    private readonly ILogger<DailySummaryAggregator> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DailySummaryAggregator(
        ILogger<DailySummaryAggregator> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow.Hour == 0)
            {
                using var scope = _serviceProvider.CreateScope();

                var redisReader = scope.ServiceProvider.GetRequiredService<IRedisReader>();
                var mongo = scope.ServiceProvider.GetRequiredService<IMongoDbWriter>();
                var publisher = scope.ServiceProvider.GetRequiredService<ITrafficDataPublisher>();

                var intersectionIds = await mongo.GetAllIntersectionIdsAsync();

                foreach (var id in intersectionIds)
                {
                    var summary = await redisReader.ComputeDailySummaryAsync(id);
                    if (summary != null)
                    {
                        await mongo.InsertDailySummaryAsync(summary);
                        await publisher.PublishAsync(summary);
                        _logger.LogInformation("Daily summary for intersection {IntersectionId} processed successfully.", id);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
