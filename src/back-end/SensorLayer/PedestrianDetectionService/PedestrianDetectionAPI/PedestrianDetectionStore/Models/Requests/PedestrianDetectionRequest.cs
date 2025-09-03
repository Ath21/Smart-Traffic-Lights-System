using System;

namespace PedestrianDetectionStore.Models.Requests;

public class PedestrianDetectionRequest
{
    public int Count { get; set; }
    public DateTime? Timestamp { get; set; }
}