namespace Social.Core.Ports.Incomming
{
    public interface IChatUseCases
    {
        Task<Chat> CreateChat(Profile creator, List<Profile> participants);
        Task<ChatMessage> SendMessage(Guid chatId, Profile sender, string content);
        Task<ChatMessage> SendImage(
            Guid chatId,
            Guid messageId,
            Profile sender,
            string fileName,
            string contentType,
            byte[] data
        );
        Task<ChatMessage> EditMessage(
            Guid chatId,
            Guid messageId,
            Profile editor,
            string newContent
        );
        Task DeleteMessage(Guid chatId, Guid messageId, Profile deleter);
        Task<List<ChatMessage>> GetMessages(Guid chatId, Profile requester, int count, int offset);
    }
}
