namespace Social.Core.Ports.Outgoing
{
    public interface INotificationSender
    {
        void SendNotification(Profile recipient, string message);
    }
}
