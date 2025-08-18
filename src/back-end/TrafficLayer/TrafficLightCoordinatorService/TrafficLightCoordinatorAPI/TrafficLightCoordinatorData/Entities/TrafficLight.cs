// src/Coordinator.Infrastructure/Entities/TrafficLightEntity.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightCoordinatorData.Entities;

public class TrafficLight
{
    public Guid Id { get; set; }                      // light_id (PK)
    public Guid IntersectionId { get; set; }          // (FK)
    public string CurrentState { get; set; } = "red"; // free text/state machine key
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Nav
    public Intersection? Intersection { get; set; }
}