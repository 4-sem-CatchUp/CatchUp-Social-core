namespace Social.Core
{
    public class Subscription
    {
        public Profile Subscriber { get; private set; }
        public Profile Publisher { get; private set; }
        public DateTime SubscribedOn { get; private set; }

        /// <summary>
        /// Creates a new subscription from the specified subscriber to the specified publisher and records the creation time.
        /// </summary>
        /// <param name="subscriber">The profile that is subscribing.</param>
        /// <param name="publisher">The profile being subscribed to.</param>
        public Subscription(Profile subscriber, Profile publisher)
        {
            Subscriber = subscriber;
            Publisher = publisher;
            SubscribedOn = DateTime.UtcNow;
        }
    }
}
