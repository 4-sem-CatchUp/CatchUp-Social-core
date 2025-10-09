namespace Social.Core.Ports.Incomming
{
    public interface IChatUseCases
    {
        /// <summary>
/// Creates a new chat with the specified creator and participants.
/// </summary>
/// <param name="creator">Profile that will be recorded as the chat creator.</param>
/// <param name="participants">List of profiles to include as participants in the chat.</param>
/// <returns>The created Chat object.</returns>
Task<Chat> CreateChat(Profile creator, List<Profile> participants);
        /// <summary>
/// Sends a text message to the specified chat from the given sender.
/// </summary>
/// <param name="chatId">Identifier of the chat to receive the message.</param>
/// <param name="sender">Profile of the user sending the message, used to authorize and attribute the message.</param>
/// <param name="content">Text content of the message.</param>
/// <returns>The created <see cref="ChatMessage"/> representing the sent message.</returns>
Task<ChatMessage> SendMessage(Guid chatId, Profile sender, string content);
        /// <summary>
        /// Sends an image to the specified chat and associates it with the given message identifier.
        /// </summary>
        /// <param name="chatId">Identifier of the chat to receive the image.</param>
        /// <param name="messageId">Identifier of the message to associate with the image.</param>
        /// <param name="sender">Profile of the user sending the image.</param>
        /// <param name="fileName">Original filename of the image.</param>
        /// <param name="contentType">MIME content type of the image (e.g., "image/png").</param>
        /// <param name="data">Binary contents of the image file.</param>
        /// <returns>The chat message representing the sent image.</returns>
        Task<ChatMessage> SendImage(
            Guid chatId,
            Guid messageId,
            Profile sender,
            string fileName,
            string contentType,
            byte[] data
        );
        /// <summary>
        /// Updates the content of a message in the specified chat.
        /// </summary>
        /// <param name="chatId">Identifier of the chat containing the message.</param>
        /// <param name="messageId">Identifier of the message to edit.</param>
        /// <param name="editor">Profile performing the edit (used for authorization).</param>
        /// <param name="newContent">New text content to replace the message's existing content.</param>
        /// <returns>The updated ChatMessage reflecting the new content.</returns>
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