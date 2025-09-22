namespace Social.Core.Ports.Incomming
{
    public interface IChatUseCases
    {
        Task CreateChat(Profile creator, List<Profile> participants);
        Task SendMessage(Guid chatId, Profile sender, string content);
        Task EditMessage(Guid chatId, Guid messageId, Profile editor, string newContent);
        Task DeleteMessage(Guid chatId, Guid messageId, Profile deleter);
        Task GetMessages(Guid chatId, Profile requester, int count, int offset);
    }
}
