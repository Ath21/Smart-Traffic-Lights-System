using System;

namespace IntersectionControllerStore.Models.Dtos;

public class IntersectionDto
{
    public Guid IntersectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
}
