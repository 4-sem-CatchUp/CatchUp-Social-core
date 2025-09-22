using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class SubscriptionService : ISubscribeUseCases
    {
        private readonly List<Subscription> _subscriptions = new();
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly INotificationSender _notificationSender;

        /// <summary>
        /// Initializes a new instance of <see cref="SubscriptionService"/> with the required dependencies.
        /// </summary>
        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            INotificationSender notificationSender
        )
        {
            _subscriptionRepository = subscriptionRepository;
            _notificationSender = notificationSender;
        }

        /// <summary>
        /// Sends the given message to every Profile subscribed to the specified publisher.
        /// </summary>
        /// <param name="Subscriber">The publisher whose subscribers should receive the message (the parameter name is `Subscriber` for historical reasons).</param>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task that completes after notification dispatch has been initiated for all matching subscriptions.</returns>
        public async Task Notify(Profile Subscriber, string message)
        {
            var subscriptions = _subscriptions.Where(s => s.Publisher.Id == Subscriber.Id).ToList();
            foreach (var subscription in subscriptions)
            {
                _notificationSender.SendNotification(subscription.Subscriber, message);
            }
        }

        /// <summary>
        /// Creates a subscription from <paramref name="subscriber"/> to <paramref name="publisher"/>,
        /// stores it in the service's in-memory list and persists it via the subscription repository.
        /// </summary>
        /// <param name="subscriber">The profile that will follow/subscribe to the publisher.</param>
        /// <param name="publisher">The profile being followed/subscribed to.</param>
        public void Subscribe(Profile subscriber, Profile publisher)
        {
            var subscription = new Subscription(subscriber, publisher);
            _subscriptions.Add(subscription);
            _subscriptionRepository.Add(subscription);
        }

        /// <summary>
        /// Removes an existing subscription where <paramref name="subscriber"/> is subscribed to <paramref name="publisher"/>.
        /// The subscription is removed from the in-memory list and from the repository.
        /// </summary>
        /// <param name="subscriber">The profile that is currently subscribed.</param>
        /// <param name="publisher">The profile being unsubscribed from.</param>
        /// <exception cref="InvalidOperationException">Thrown if no matching subscription exists.</exception>
        public void Unsubscribe(Profile subscriber, Profile publisher)
        {
            var subscription = _subscriptions.FirstOrDefault(s =>
                s.Subscriber.Id == subscriber.Id && s.Publisher.Id == publisher.Id
            );
            if (subscription != null)
            {
                _subscriptions.Remove(subscription);
                _subscriptionRepository.Remove(subscription);
            }
            else
            {
                throw new InvalidOperationException("Subscription not found.");
            }
        }
    }
}
