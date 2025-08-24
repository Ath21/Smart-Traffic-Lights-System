using System;

namespace NotificationStore.Models;

public class PublicNoticeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
}
