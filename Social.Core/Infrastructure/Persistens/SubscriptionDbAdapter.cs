using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class SubscriptionDbAdapter : ISubscriptionRepository
    {
        private readonly SocialDbContext _context;

        public SubscriptionDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        public async Task Add(Subscription subscription)
        {
            // Fetch existing profiles from the database
            var subscriberEntity = await _context.Profiles.FindAsync(subscription.Subscriber.Id);
            var publisherEntity = await _context.Profiles.FindAsync(subscription.Publisher.Id);

            if (subscriberEntity == null || publisherEntity == null)
                throw new InvalidCastException(
                    "Subscriber or Publisher does not exist in the database."
                );

            // Check for existing subscription
            var exists = await _context.Subscriptions.AnyAsync(s =>
                s.SubscriberId == subscription.Subscriber.Id
                && s.PublisherId == subscription.Publisher.Id
            );

            if (exists)
                throw new DbUpdateException("Subscription already exists.");

            // Create and add the subscription entity
            var entity = subscription.ToEntity();
            entity.Subscriber = subscriberEntity;
            entity.Publisher = publisherEntity;

            _context.Subscriptions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Remove(Subscription subscription)
        {
            // Find the subscription entity by its ID
            var entity = await _context.Subscriptions.FindAsync(subscription.Id);
            if (entity == null)
                return; // Subscription does not exist, nothing to remove
            // If found, remove it
            if (entity != null)
            {
                _context.Subscriptions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }

    public static class SubscriptionMapper
    {
        // Map from Domain to Entity
        public static SubscriptionEntity ToEntity(this Subscription subscription)
        {
            return new SubscriptionEntity
            {
                Id = subscription.Id,
                SubscriberId = subscription.Subscriber.Id,
                Subscriber = subscription.Subscriber.ToEntity(),
                PublisherId = subscription.Publisher.Id,
                Publisher = subscription.Publisher.ToEntity(),
                SubscribedOn = subscription.SubscribedOn,
            };
        }

        // Map from Entity to Domain
        public static Subscription ToDomain(this SubscriptionEntity entity)
        {
            var subscriber = new Subscription(
                entity.Subscriber.ToDomain(),
                entity.Publisher.ToDomain()
            );
            typeof(Subscription).GetProperty("Id")!.SetValue(subscriber, entity.Id);
            typeof(Subscription)
                .GetProperty("SubscribedOn")!
                .SetValue(subscriber, entity.SubscribedOn);
            return subscriber;
        }
    }
}
