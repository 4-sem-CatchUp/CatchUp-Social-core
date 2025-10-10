namespace Social.Core
{
    public class Subscription
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Profile Subscriber { get; private set; }
        public Profile Publisher { get; private set; }
        public DateTime SubscribedOn { get; private set; }

        public Subscription(Profile subscriber, Profile publisher)
        {
            Subscriber = subscriber;
            Publisher = publisher;
            SubscribedOn = DateTime.UtcNow;
        }
    }
}
