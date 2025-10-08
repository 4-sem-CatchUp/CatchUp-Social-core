using System;
using Moq;
using NUnit.Framework;
using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Outgoing;

namespace SocialCoreTests.CoreTests.ApplicationTests
{
    [TestFixture]
    public class SubscriptionServiceTests
    {
        private Mock<ISubscriptionRepository> _subscriptionRepoMock;
        private Mock<INotificationSender> _notificationSenderMock;
        private SubscriptionService _service;
        private Profile _subscriber1;
        private Profile _subscriber2;
        private Profile _publisher;

        [SetUp]
        public void Setup()
        {
            _subscriptionRepoMock = new Mock<ISubscriptionRepository>();
            _notificationSenderMock = new Mock<INotificationSender>();
            _service = new SubscriptionService(
                _subscriptionRepoMock.Object,
                _notificationSenderMock.Object
            );

            _subscriber1 = new Profile("Alice");
            _subscriber2 = new Profile("Charlie");
            _publisher = new Profile("Bob");
        }

        [Test]
        public void Subscribe_AddsSubscriptionAndCallsRepo()
        {
            _service.Subscribe(_subscriber1, _publisher);

            _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        }

        [Test]
        public void Unsubscribe_RemovesExistingSubscription()
        {
            _service.Subscribe(_subscriber1, _publisher);

            _service.Unsubscribe(_subscriber1, _publisher);

            _subscriptionRepoMock.Verify(r => r.Remove(It.IsAny<Subscription>()), Times.Once);
        }

        [Test]
        public void Unsubscribe_NonExisting_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _service.Unsubscribe(_subscriber1, _publisher)
            );
        }

        [Test]
        public void Notify_SendsMessageToAllSubscribers()
        {
            // Arrange
            _service.Subscribe(_subscriber1, _publisher);
            _service.Subscribe(_subscriber2, _publisher);
            string message = "New post available!";

            // Act
            _service.Notify(_publisher, message);

            // Assert
            _notificationSenderMock.Verify(
                n => n.SendNotification(_subscriber1, message),
                Times.Once
            );
            _notificationSenderMock.Verify(
                n => n.SendNotification(_subscriber2, message),
                Times.Once
            );
        }

        [Test]
        public void Notify_DoesNotSendMessageToNonSubscribers()
        {
            var nonSubscriber = new Profile("NonSubscriber");
            _service.Subscribe(_subscriber1, _publisher);
            string message = "Hello!";

            _service.Notify(_publisher, message);

            _notificationSenderMock.Verify(
                n => n.SendNotification(nonSubscriber, It.IsAny<string>()),
                Times.Never
            );
        }

        [Test]
        public void Subscription_SetsSubscribedOnToUtcNow()
        {
            var subscription = new Subscription(_subscriber1, _publisher);

            Assert.That(subscription.SubscribedOn, Is.LessThanOrEqualTo(DateTime.UtcNow));
        }
    }
}
