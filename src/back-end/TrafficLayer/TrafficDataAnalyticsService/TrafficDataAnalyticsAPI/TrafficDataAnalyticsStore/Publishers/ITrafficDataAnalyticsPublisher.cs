using System;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Publishers;

public interface ITrafficDataAnalyticsPublisher
{
    Task PublishDailySummaryAsync(DailySummaryDto dailySummaryDto);
    Task PublishCongestionAlertAsync(CongestionAlertDto congestionAlertDto);
}
