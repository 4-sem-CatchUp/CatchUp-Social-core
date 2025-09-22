namespace Social.Core.Ports.Outgoing
{
    public interface ISubscriptionRepository
    {
        void Add(Subscription subscription);
        void Remove(Subscription subscription);
    }
}
