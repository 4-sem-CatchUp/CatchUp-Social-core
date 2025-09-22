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
        /// Initializes a new instance of <see cref="SubscriptionService"/> with required dependencies.
        /// </summary>
        public SubscriptionService(ISubscriptionRepository subscriptionRepository, INotificationSender notificationSender)
        {
            _subscriptionRepository = subscriptionRepository;
            _notificationSender = notificationSender;
        }

        /// <summary>
        /// Sends the specified message to every profile subscribed to the given publisher.
        /// </summary>
        /// <param name="Subscriber">The publisher whose subscribers should receive the notification.</param>
        /// <param name="message">The notification text to deliver to each subscriber.</param>
        /// <returns>A task that completes after notifications have been dispatched to all matching subscribers.</returns>
        public async Task Notify(Profile Subscriber, string message)
        {
            var subscriptions = _subscriptions.Where(s => s.Publisher.Id == Subscriber.Id).ToList();
            foreach (var subscription in subscriptions)
            {
                _notificationSender.SendNotification(subscription.Subscriber, message);
            }
        }

        /// <summary>
        /// Creates and persists a subscription where <paramref name="subscriber"/> follows <paramref name="publisher"/>,
        /// and caches it in the service's in-memory subscription list.
        /// </summary>
        /// <param name="subscriber">The profile that will follow (the subscriber).</param>
        /// <param name="publisher">The profile to be followed (the publisher).</param>
        public void Subscribe(Profile subscriber, Profile publisher)
        {
            var subscription = new Subscription(subscriber, publisher);
            _subscriptions.Add(subscription);
            _subscriptionRepository.Add(subscription);
        }

        /// <summary>
        /// Removes an existing subscription where <paramref name="subscriber"/> follows <paramref name="publisher"/>.
        /// </summary>
        /// <param name="subscriber">The profile that is unsubscribing.</param>
        /// <param name="publisher">The profile being unsubscribed from.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if no matching subscription exists.</exception>
        public void Unsubscribe(Profile subscriber, Profile publisher)
        {
            var subscription = _subscriptions.FirstOrDefault(s => s.Subscriber.Id == subscriber.Id && s.Publisher.Id == publisher.Id);
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
