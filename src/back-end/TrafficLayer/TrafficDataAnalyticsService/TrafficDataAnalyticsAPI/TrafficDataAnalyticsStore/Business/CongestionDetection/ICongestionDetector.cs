using System;

namespace TrafficDataAnalyticsStore.Business.CongestionDetection;

public interface ICongestionDetector
{
    bool isCongested(float avgWaitTime, int totalVehicleCount);
    string GetSeverity(float avgWaitTime, int totalVehicleCount);
}
