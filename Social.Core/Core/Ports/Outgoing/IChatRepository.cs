namespace Social.Core.Ports.Outgoing
{
    public interface IChatRepository
    {
        Task CreateChat(Chat chat);
        Task<Chat> GetChat(Guid chatId);
        Task<ChatMessage> GetMessage(Guid chatId, Guid messageId);
        Task<List<ChatMessage>> GetMessages(Guid chatId, int count, int offset);
        Task AddMessage(Guid chatId, ChatMessage message);
        Task UpdateMessage(Guid chatId, ChatMessage message);
        Task DeleteMessage(Guid chatId, Guid messageId);
        Task UpdateChat(Chat chat);
    }
}
