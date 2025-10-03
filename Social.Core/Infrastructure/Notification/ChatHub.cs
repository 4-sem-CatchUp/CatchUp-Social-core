using Microsoft.AspNetCore.SignalR;
using Social.Core;
using Social.Core.Ports.Outgoing;

namespace Social.Social.Infrastructure.Notification
{
    public class ChatHub : Hub, IChatNotifier
    {
        public Task NotifyChatCreated(Chat chat)
        {
            throw new NotImplementedException();
        }

        public async Task NotifyMessageSent(ChatMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
