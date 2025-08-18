using System;
using System.Text.Json;

namespace TrafficLightCoordinatorStore.Models;

public class TrafficConfigurationReadDto
{
    public Guid ConfigId { get; set; }
    public Guid IntersectionId { get; set; }
    public JsonElement Pattern { get; set; }
    public DateTime EffectiveFrom { get; set; }
}
