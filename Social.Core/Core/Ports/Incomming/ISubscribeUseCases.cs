namespace Social.Core.Ports.Incomming
{
    public interface ISubscribeUseCases
    {
        /// <summary>
/// Establishes a subscription relationship so <paramref name="subscriber"/> follows or receives updates from <paramref name="publisher"/>.
/// </summary>
/// <param name="subscriber">The profile that will become the subscriber (the follower).</param>
/// <param name="publisher">The profile that will be subscribed to (the publisher).</param>
void Subscribe(Profile subscriber, Profile publisher);
        /// <summary>
/// Removes an existing subscription relationship where <paramref name="subscriber"/> is subscribed to <paramref name="publisher"/>.
/// </summary>
/// <param name="subscriber">The profile that currently follows or is subscribed and will be unsubscribed.</param>
/// <param name="publisher">The profile that is being unsubscribed from.</param>
void Unsubscribe(Profile subscriber, Profile publisher);
        /// <summary>
/// Asynchronously delivers a notification message to the specified subscriber.
/// </summary>
/// <param name="Subscriber">The profile that should receive the notification.</param>
/// <param name="message">The notification text to send.</param>
/// <returns>A task that represents the asynchronous send operation.</returns>
Task Notify(Profile Subscriber, string message);
    }
}
