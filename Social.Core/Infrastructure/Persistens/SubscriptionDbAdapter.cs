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

        /// <summary>
        /// Initializes a new instance of <see cref="SubscriptionDbAdapter"/> using the provided database context.
        /// </summary>
        /// <param name="context">The <see cref="SocialDbContext"/> used for subscription data access.</param>
        public SubscriptionDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds the given domain subscription to the database and persists the change.
        /// </summary>
        /// <param name="subscription">The domain subscription to create and store.</param>
        /// <exception cref="InvalidCastException">Thrown when the subscriber or publisher profile does not exist in the database.</exception>
        /// <exception cref="DbUpdateException">Thrown when a subscription between the specified subscriber and publisher already exists.</exception>
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

        /// <summary>
        /// Removes the database subscription identified by the Id of the provided domain subscription.
        /// </summary>
        /// <param name="subscription">Domain subscription whose Id is used to locate and remove the corresponding database entity.</param>
        /// <remarks>If no matching subscription exists, the method completes without error or side effects.</remarks>
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
        /// <summary>
        /// Converts a domain Subscription into its corresponding persistence SubscriptionEntity.
        /// </summary>
        /// <param name="subscription">The domain subscription to map.</param>
        /// <returns>A SubscriptionEntity representing the given domain subscription, including IDs, navigation properties, and subscribed timestamp.</returns>
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

        /// <summary>
        /// Creates a domain <see cref="Subscription"/> from a data <see cref="SubscriptionEntity"/>.
        /// </summary>
        /// <param name="entity">The subscription entity to convert. Must contain populated Subscriber and Publisher entities.</param>
        /// <returns>A <see cref="Subscription"/> domain instance with its Id and SubscribedOn set from the entity.</returns>
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