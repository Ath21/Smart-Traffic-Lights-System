using System;

namespace TrafficLightCoordinatorStore.Consumers;

internal static class PatternBuilder
{
    public static string For(string kind, bool active)
    {
        if (!active) return "NS_Green(30)->EW_Green(30)";
        return kind switch
        {
            "emergency"        => "NS_Green(45)->EW_Green(15)",
            "public_transport" => "NS_Green(38)->EW_Green(22)",
            "pedestrian"       => "PED_Walk(20)->All_Red(5)->Cycle(35)",
            "cyclist"          => "Bike_Green(20)->All_Red(5)->Cycle(35)",
            _                  => "NS_Green(30)->EW_Green(30)"
        };
    }
}
