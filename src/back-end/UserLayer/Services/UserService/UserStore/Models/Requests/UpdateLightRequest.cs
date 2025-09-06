using System;

namespace UserStore.Models.Requests;

public class UpdateLightRequest
{
    public string CurrentState { get; set; } = string.Empty;
}