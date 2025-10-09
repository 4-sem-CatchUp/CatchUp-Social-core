namespace Social.Core.Ports.Outgoing
{
    public interface ISubscriptionRepository
    {
        /// <summary>
/// Adds the specified subscription to the repository.
/// </summary>
/// <param name="subscription">The subscription to add.</param>
Task Add(Subscription subscription);
        /// <summary>
/// Removes a subscription from the repository.
/// </summary>
/// <param name="subscription">The subscription to remove.</param>
Task Remove(Subscription subscription);
    }
}