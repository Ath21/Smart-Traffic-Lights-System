using System;
using TrafficDataAnalyticsStore.Models;

namespace TrafficDataAnalyticsStore.Business.Congestion;

public interface ICongestionAlertService
{
    Task<List<CongestionAlertDto>> GetActiveAlertsAsync();
}
