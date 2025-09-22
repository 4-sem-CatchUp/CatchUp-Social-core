namespace Social.Core.Ports.Incomming
{
    public interface ISubscribeUseCases
    {
        void Subscribe(Profile subscriber, Profile publisher);
        void Unsubscribe(Profile subscriber, Profile publisher);
        Task Notify(Profile Subscriber, string message);
    }
}
