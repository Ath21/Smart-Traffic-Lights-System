using System;

namespace IntersectionControllerStore.Models.Responses;

public class IntersectionResponse
{
    public Guid IntersectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
}