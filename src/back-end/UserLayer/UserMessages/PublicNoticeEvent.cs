namespace UserMessages;

// notification.event.public_notice
public record PublicNoticeEvent(
    Guid NoticeId,
    string Title,
    string Message,
    string TargetAudience,
    DateTime PublishedAt
);
