namespace Social.Core.Ports.Incomming
{
    public interface ISubscribeUseCases
    {
        /// <summary>
/// Creates a subscription relationship so the <paramref name="subscriber"/> will receive updates from the <paramref name="publisher"/>.
/// </summary>
/// <param name="subscriber">The profile that will start following the publisher.</param>
/// <param name="publisher">The profile being followed.</param>
void Subscribe(Profile subscriber, Profile publisher);
        /// <summary>
/// Removes the subscription relationship where <paramref name="subscriber"/> is subscribed to <paramref name="publisher"/>.
/// </summary>
/// <param name="subscriber">The profile that will stop receiving updates from the publisher.</param>
/// <param name="publisher">The profile being unsubscribed from.</param>
void Unsubscribe(Profile subscriber, Profile publisher);
        /// <summary>
/// Asynchronously sends a notification message to the specified subscriber.
/// </summary>
/// <param name="Subscriber">The profile that should receive the notification.</param>
/// <param name="message">The text of the notification to deliver.</param>
/// <returns>A task that completes when the notification operation has finished.</returns>
Task Notify(Profile Subscriber, string message);
    }
}
