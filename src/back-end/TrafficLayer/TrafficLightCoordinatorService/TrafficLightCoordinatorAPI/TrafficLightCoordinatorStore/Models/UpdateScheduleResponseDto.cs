using System;

namespace TrafficLightCoordinatorStore.Models;

public class UpdateScheduleResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UpdateScheduleResponseDto() { }
    public UpdateScheduleResponseDto(bool success, string message)
    { Success = success; Message = message; }
}
