namespace Social.Core.Ports.Outgoing
{
    public interface IChatRepository
    {
        Task CreateChat(Chat chat);
        Task<Chat> GetChat(Guid chatId);
        Task<ChatMessage> GetMessage(Guid chatId, Guid messageId);
        Task<List<ChatMessage>> GetMessages(Guid chatId, int count, int offset);
        /// <summary>
/// Adds a new message to the specified chat.
/// </summary>
/// <param name="chatId">The unique identifier of the chat to which the message will be added.</param>
/// <param name="message">The message to add to the chat.</param>
Task AddMessage(Guid chatId, ChatMessage message);
        /// <summary>
/// Updates an existing message in the specified chat using the data from <paramref name="message"/>.
/// </summary>
/// <param name="chatId">Identifier of the chat that contains the message to update.</param>
/// <param name="message">The updated message data; the message's Id identifies which message to replace.</param>
Task UpdateMessage(Guid chatId, ChatMessage message);
        /// <summary>
/// Deletes a specific message from the specified chat.
/// </summary>
/// <param name="chatId">Unique identifier of the chat that contains the message.</param>
/// <param name="messageId">Unique identifier of the message to delete.</param>
Task DeleteMessage(Guid chatId, Guid messageId);
        /// <summary>
/// Updates an existing chat entity with the values from the provided Chat object.
/// </summary>
/// <param name="chat">The Chat containing the updated data; its Id identifies the chat to update.</param>
Task UpdateChat(Chat chat);
    }
}