namespace Social.Core.Ports.Outgoing
{
    public interface IChatRepository
    {
        Chat CreateChat(Chat chat);
        Chat GetChat(Guid chatId);
        void UpdateChat(Chat chat);
        void DeleteChat(Guid chatId);
    }
}
