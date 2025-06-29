using System;
using AutoMapper;
using TrafficDataAnalyticsData.Collections;
using TrafficDataAnalyticsStore.Business.Redis;
using TrafficDataAnalyticsStore.Publishers;
using TrafficDataAnalyticsStore.Repository;

namespace TrafficDataAnalyticsStore.Business.DailySum;

public class DailySummaryAggregator : BackgroundService
{
    private readonly ILogger<DailySummaryAggregator> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;

    public DailySummaryAggregator(
        ILogger<DailySummaryAggregator> logger,
        IServiceProvider serviceProvider,
        IMapper mapper)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _mapper = mapper;
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
                var publisher = scope.ServiceProvider.GetRequiredService<ITrafficDataAnalyticsPublisher>();

                var intersectionIds = await mongo.GetAllIntersectionIdsAsync();

                foreach (var id in intersectionIds)
                {
                    var summaryDto = await redisReader.ComputeDailySummaryAsync(id);
                    if (summaryDto != null)
                    {
                        var summary = _mapper.Map<DailySummary>(summaryDto);
                        await mongo.InsertDailySummaryAsync(summary);
                        await publisher.PublishDailySummaryAsync(summaryDto);
                        _logger.LogInformation("Daily summary for intersection {IntersectionId} processed successfully.", id);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
