using System;
using System.Text.Json;

namespace TrafficLightCoordinatorStore.Models;


public class TrafficConfigurationCreateDto
{
    public Guid IntersectionId { get; set; }
    public JsonElement Pattern { get; set; }
    public DateTime EffectiveFrom { get; set; }
}