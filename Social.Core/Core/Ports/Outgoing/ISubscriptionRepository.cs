namespace Social.Core.Ports.Outgoing
{
    public interface ISubscriptionRepository
    {
        Task Add(Subscription subscription);
        Task Remove(Subscription subscription);
    }
}
