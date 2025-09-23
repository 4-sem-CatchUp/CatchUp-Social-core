using Social.Core;
using Social.Core.Ports.Outgoing;

namespace Social.Social.Infrastructure.Notification
{
    public class NotificationHub : INotificationSender
    {
        public void SendNotification(Profile recipient, string message)
        {
            throw new NotImplementedException();
        }
    }
}
