// src/Coordinator.Infrastructure/Entities/TrafficLightEntity.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightCoordinatorData.Entities;

public class TrafficLight
{
    [Key] public Guid LightId { get; set; }

    [ForeignKey(nameof(Intersection))]
    public Guid IntersectionId { get; set; }

    [Required, MaxLength(32)]
    public string CurrentState { get; set; } = "Red"; // e.g., Red/Amber/Green

    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Intersection? Intersection { get; set; }
}
