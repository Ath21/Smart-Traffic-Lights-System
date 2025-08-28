using MassTransit;
using UserMessages;

namespace UserStore.Consumers.Usr;

public class PublicNoticeConsumer : IConsumer<PublicNoticeEvent>
{
    private readonly ILogger<PublicNoticeConsumer> _logger;

    private const string ServiceTag = "[" + nameof(PublicNoticeConsumer) + "]";

    public PublicNoticeConsumer(ILogger<PublicNoticeConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // notification.event.public_notice
    public Task Consume(ConsumeContext<PublicNoticeEvent> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received PublicNotice: {Title} - {Message} for {Audience}",
            ServiceTag, msg.Title, msg.Message, msg.TargetAudience
        );

        // TODO: store notice in DB or cache for UI display
        return Task.CompletedTask;
    }
}
