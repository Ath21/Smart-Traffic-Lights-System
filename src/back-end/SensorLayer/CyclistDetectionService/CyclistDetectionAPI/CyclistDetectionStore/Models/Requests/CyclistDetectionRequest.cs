using System;

namespace CyclistDetectionStore.Models.Requests;


public class CyclistDetectionRequest
{
    public int Count { get; set; }
    public DateTime? Timestamp { get; set; }
}
