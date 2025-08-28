namespace UserStore.Publishers.Traffic;

public interface ITrafficPublisher
{
    // traffic.light.control.{intersection_id}.{light_id}
    Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState);
    // traffic.light.update.{intersection_id}
    Task PublishUpdateAsync(Guid intersectionId, Guid lightId, string currentState);
}
