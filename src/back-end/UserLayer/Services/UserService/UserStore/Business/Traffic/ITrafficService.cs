namespace UserStore.Business.Traffic;

public interface ITrafficService
{
    Task ControlLightAsync(Guid intersectionId, Guid lightId, string newState);
    Task UpdateLightAsync(Guid intersectionId, Guid lightId, string currentState);
}