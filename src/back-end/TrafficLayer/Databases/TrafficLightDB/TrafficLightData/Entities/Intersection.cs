using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

    public class Intersection
    {
        public int IntersectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = "{}"; // JSON
        public string? Description { get; set; }
        public DateTime InstalledAt { get; set; }
        public string Status { get; set; } = "Active";

        public ICollection<TrafficLight> TrafficLights { get; set; } = new List<TrafficLight>();
        public ICollection<TrafficConfiguration> Configurations { get; set; } = new List<TrafficConfiguration>();
    }
