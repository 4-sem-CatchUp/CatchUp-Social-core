using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class SubscriptionService : ISubscribeUseCases
    {
        private readonly List<Subscription> _subscriptions = new();
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly INotificationSender _notificationSender;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            INotificationSender notificationSender
        )
        {
            _subscriptionRepository = subscriptionRepository;
            _notificationSender = notificationSender;
        }

        public async Task Notify(Profile Subscriber, string message)
        {
            var subscriptions = _subscriptions.Where(s => s.Publisher.Id == Subscriber.Id).ToList();
            foreach (var subscription in subscriptions)
            {
                _notificationSender.SendNotification(subscription.Subscriber, message);
            }
        }

        public void Subscribe(Profile subscriber, Profile publisher)
        {
            var subscription = new Subscription(subscriber, publisher);
            _subscriptions.Add(subscription);
            _subscriptionRepository.Add(subscription);
        }

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
