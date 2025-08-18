// src/Coordinator.Infrastructure/Entities/IntersectionEntity.cs
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace TrafficLightCoordinatorData.Entities;

public class Intersection
{
    [Key] public Guid IntersectionId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // GEOGRAPHY(Point, 4326). If you prefer JSON instead, see DbContext note below.
    public Point? Location { get; set; }

    public string? Description { get; set; }

    public DateTime InstalledAt { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "inactive";

    // Navigation
    public ICollection<TrafficLight> TrafficLights { get; set; } = new List<TrafficLight>();
    public ICollection<TrafficConfiguration> TrafficConfigurations { get; set; } = new List<TrafficConfiguration>();
}
