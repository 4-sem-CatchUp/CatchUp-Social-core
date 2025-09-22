namespace Social.Core.Ports.Outgoing
{
    public interface ISubscriptionRepository
    {
        /// <summary>
/// Persists or registers a subscription.
/// </summary>
/// <param name="subscription">The subscription entity to add to the repository.</param>
void Add(Subscription subscription);
        /// <summary>
/// Removes the specified subscription from the repository.
/// </summary>
/// <param name="subscription">The subscription entity to remove.</param>
void Remove(Subscription subscription);

    }
}
