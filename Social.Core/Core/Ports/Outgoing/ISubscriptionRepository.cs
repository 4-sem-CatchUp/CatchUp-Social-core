namespace Social.Core.Ports.Outgoing
{
    public interface ISubscriptionRepository
    {
        /// <summary>
/// Adds the given Subscription to the repository.
/// </summary>
/// <param name="subscription">The subscription entity to persist. Implementations should store or register this subscription so it becomes retrievable by repository queries.</param>
void Add(Subscription subscription);
        /// <summary>
/// Removes the specified subscription from the repository.
/// </summary>
/// <param name="subscription">The subscription to remove.</param>
void Remove(Subscription subscription);

    }
}
