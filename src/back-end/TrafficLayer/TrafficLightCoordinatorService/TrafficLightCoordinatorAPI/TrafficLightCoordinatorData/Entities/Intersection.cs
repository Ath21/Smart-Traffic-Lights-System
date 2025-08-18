// src/Coordinator.Infrastructure/Entities/IntersectionEntity.cs
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace TrafficLightCoordinatorData.Entities;

public class Intersection
{
    public Guid Id { get; set; }                      // intersection_id (PK)
    public string Name { get; set; } = string.Empty;
    public Point? Location { get; set; }              // GEOGRAPHY(Point,4326) or null
    public string? Description { get; set; }
    public DateTime? InstalledAt { get; set; }
    public string Status { get; set; } = "active";

    // Nav
    public ICollection<TrafficLight> TrafficLights { get; set; } = new List<TrafficLight>();
    public ICollection<TrafficConfiguration> Configurations { get; set; } = new List<TrafficConfiguration>();
}