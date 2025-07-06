using System;

namespace TrafficDataAnalyticsStore.Business.CongestionDetection;

public class CongestionDetector : ICongestionDetector
{
    private const float WaitThresholdHigh = 45f;
    private const float WaitThresholdMedium = 30f;
    private const int VehicleCountHigh = 10000;
    private const int VehilceCountMedium = 5000;

    public string GetSeverity(float avgWaitTime, int totalVehicleCount)
    {
        if (avgWaitTime > WaitThresholdHigh || totalVehicleCount > VehicleCountHigh)
            return "HIGH";

        if (avgWaitTime > WaitThresholdMedium || totalVehicleCount > VehilceCountMedium)
            return "MEDIUM";

        return "LOW";
    }

    public bool isCongested(float avgWaitTime, int totalVehicleCount)
    {
        return avgWaitTime > WaitThresholdMedium || totalVehicleCount > VehilceCountMedium;
    }
}
