namespace Social.Core
{
    public class Subscription
    {
        public Profile Subscriber { get; private set; }
        public Profile Publisher { get; private set; }
        public DateTime SubscribedOn { get; private set; }

        /// <summary>
        /// Creates a new subscription representing that <paramref name="subscriber"/> follows <paramref name="publisher"/>.
        /// </summary>
        /// <param name="subscriber">The profile that subscribes (follower).</param>
        /// <param name="publisher">The profile being subscribed to (followed).</param>
        public Subscription(Profile subscriber, Profile publisher)
        {
            Subscriber = subscriber;
            Publisher = publisher;
            SubscribedOn = DateTime.UtcNow;
        }
    }
}
