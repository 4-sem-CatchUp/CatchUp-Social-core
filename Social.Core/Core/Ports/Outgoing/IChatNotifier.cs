namespace Social.Core.Ports.Outgoing
{
    public interface IChatNotifier
    {
        Task NotifyMessageSent(ChatMessage message);
        Task NotifyChatCreated(Chat chat);
    }
}
