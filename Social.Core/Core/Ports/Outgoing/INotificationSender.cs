namespace Social.Core.Ports.Outgoing
{
    public interface INotificationSender
    {
        /// <summary>
/// Sends a notification message to the specified recipient.
/// </summary>
/// <param name="recipient">The profile of the notification recipient.</param>
/// <param name="message">The notification content to deliver.</param>
void SendNotification(Profile recipient, string message);
    }
}
