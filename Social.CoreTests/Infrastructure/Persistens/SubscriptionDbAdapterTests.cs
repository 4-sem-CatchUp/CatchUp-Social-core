using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Social.Core;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace SocialCoreTests.Infrastructure.Persistens
{
    [TestFixture]
    public class SubscriptionDbAdapterTests
    {
        private SocialDbContext _context;
        private SubscriptionDbAdapter _adapter;

        private Profile _subscriber;
        private Profile _publisher;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SocialDbContext(options);
            _adapter = new SubscriptionDbAdapter(_context);

            // Dummy domain profiles
            _subscriber = new Profile("subscriber1");
            _publisher = new Profile("publisher1");
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        private async Task SeedProfilesAsync()
        {
            // Tilføj profiler i databasen for navigation properties
            var subscriberEntity = new ProfileEntity { Id = _subscriber.Id, Name = "subscriber1" };
            var publisherEntity = new ProfileEntity { Id = _publisher.Id, Name = "publisher1" };

            await _context.Profiles.AddRangeAsync(subscriberEntity, publisherEntity);
            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task Add_ShouldAddSubscription_WithProfiles()
        {
            // Arrange
            await SeedProfilesAsync();
            var subscription = new Subscription(_subscriber, _publisher);

            // Act
            await _adapter.Add(subscription);

            // Assert
            var savedEntity = await _context
                .Subscriptions.Include(s => s.Subscriber)
                .Include(s => s.Publisher)
                .FirstOrDefaultAsync(s => s.Id == subscription.Id);

            Assert.That(savedEntity, Is.Not.Null, "Subscription blev ikke tilføjet til databasen.");
            Assert.That(subscription.Subscriber.Id, Is.EqualTo(savedEntity.Subscriber.Id));
            Assert.That(subscription.Publisher.Id, Is.EqualTo(savedEntity.Publisher.Id));
        }

        [Test]
        public async Task Remove_ShouldRemoveSubscription_WhenExists()
        {
            // Arrange
            await SeedProfilesAsync();
            var subscription = new Subscription(_subscriber, _publisher);
            await _adapter.Add(subscription);

            // Act
            await _adapter.Remove(subscription);

            // Assert
            var deletedEntity = await _context.Subscriptions.FindAsync(subscription.Id);
            Assert.That(deletedEntity, Is.Null, "Subscription blev ikke fjernet fra databasen.");
        }

        [Test]
        public async Task Remove_ShouldDoNothing_WhenSubscriptionDoesNotExist()
        {
            // Arrange
            await SeedProfilesAsync();
            var subscription = new Subscription(_subscriber, _publisher);

            // Act & Assert
            Assert.DoesNotThrowAsync(
                async () => await _adapter.Remove(subscription),
                "Remove kastede en undtagelse selvom subscription ikke eksisterede."
            );
        }

        [Test]
        public async Task Add_ShouldFail_WhenDuplicateSubscription()
        {
            // Arrange
            await SeedProfilesAsync();
            var subscription1 = new Subscription(_subscriber, _publisher);
            var subscription2 = new Subscription(_subscriber, _publisher); // Samme subscriber/publisher

            // Act
            await _adapter.Add(subscription1);
            var ex = Assert.ThrowsAsync<DbUpdateException>(async () =>
                await _adapter.Add(subscription2)
            );

            // Assert
            Assert.That(ex, Is.Not.Null, "DbUpdateException blev ikke kastet ved duplikat.");
        }
    }
}
