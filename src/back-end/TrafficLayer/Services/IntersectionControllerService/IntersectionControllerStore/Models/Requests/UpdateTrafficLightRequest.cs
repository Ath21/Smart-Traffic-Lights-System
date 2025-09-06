using System;

namespace IntersectionControllerStore.Models.Requests;

public class UpdateTrafficLightRequest
{
    public Guid IntersectionId { get; set; }
    public Guid LightId { get; set; }
    public string NewState { get; set; } = string.Empty; // "Red", "Green", "Yellow"
}