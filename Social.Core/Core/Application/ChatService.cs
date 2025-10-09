using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class ChatService : IChatUseCases
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatNotifier _chatNotifier;

        public ChatService(IChatRepository chatRepository, IChatNotifier chatNotifier)
        {
            _chatRepository = chatRepository;
            _chatNotifier = chatNotifier;
        }

        public async Task<Chat> CreateChat(Profile creator, List<Profile> participants)
        {
            var chat = new Chat(participants);
            chat.AddParticipant(creator);

            await _chatRepository.CreateChat(chat);
            await _chatNotifier.NotifyChatCreated(chat);

            return chat;
        }

        public async Task DeleteMessage(Guid chatId, Guid messageId, Profile deleter)
        {
            var message = await _chatRepository.GetMessage(chatId, messageId);
            if (message == null)
                throw new InvalidOperationException("Message not found");
            if (message.Sender.Id != deleter.Id)
                throw new InvalidOperationException("Only the sender can delete the message");
            await _chatRepository.DeleteMessage(chatId, messageId);
        }

        public async Task<ChatMessage> EditMessage(
            Guid chatId,
            Guid messageId,
            Profile editor,
            string newContent
        )
        {
            var message = await _chatRepository.GetMessage(chatId, messageId);
            if (message == null)
                throw new InvalidOperationException("Message not found");

            if (message.Sender.Id != editor.Id)
                throw new InvalidOperationException("Only the sender can edit the message");
            message.EditContent(newContent);
            await _chatRepository.UpdateMessage(chatId, message);

            return message;
        }

        /// <summary>
        /// Retrieves a paginated list of messages for the specified chat after verifying the requester is a participant.
        /// </summary>
        /// <param name="chatId">The identifier of the chat to fetch messages from.</param>
        /// <param name="requester">The profile of the user requesting messages; used to verify participation.</param>
        /// <param name="count">Maximum number of messages to return.</param>
        /// <param name="offset">Number of messages to skip for pagination.</param>
        /// <returns>A list of <see cref="ChatMessage"/> objects matching the requested page.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the chat does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the requester is not a participant of the chat.</exception>
        public async Task<List<ChatMessage>> GetMessages(
            Guid chatId,
            Profile requester,
            int count,
            int offset
        )
        {
            var chat = await _chatRepository.GetChat(chatId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found");

            if (!chat.Participants.Any(p => p.Id == requester.Id))
                throw new InvalidOperationException("Requester is not a participant of the chat");

            var messages = await _chatRepository.GetMessages(chatId, count, offset);

            return messages;
        }

        /// <summary>
        /// Creates a new chat message with an image attachment and sends it to the specified chat.
        /// </summary>
        /// <param name="chatId">Identifier of the target chat.</param>
        /// <param name="messageId">Identifier for the new message.</param>
        /// <param name="sender">Profile of the message sender.</param>
        /// <param name="fileName">Original filename of the image.</param>
        /// <param name="contentType">MIME type of the image (e.g., "image/png").</param>
        /// <param name="data">Binary content of the image.</param>
        /// <returns>The created <see cref="ChatMessage"/> containing the image attachment.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified chat does not exist.</exception>
        public async Task<ChatMessage> SendImage(
            Guid chatId,
            Guid messageId,
            Profile sender,
            string fileName,
            string contentType,
            byte[] data
        )
        {
            var chat = await _chatRepository.GetChat(chatId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found");
            var message = chat.SendMessage(sender, "");
            message.AddImage(fileName, contentType, data);
            await _chatRepository.AddMessage(chatId, message);
            await _chatNotifier.NotifyMessageSent(message);
            return message;
        }

        /// <summary>
        /// Sends a text message to the specified chat.
        /// </summary>
        /// <param name="chatId">The identifier of the target chat.</param>
        /// <param name="sender">The profile of the message sender.</param>
        /// <param name="content">The text content of the message.</param>
        /// <returns>The created <see cref="ChatMessage"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the chat with <paramref name="chatId"/> cannot be found.</exception>
        public async Task<ChatMessage> SendMessage(Guid chatId, Profile sender, string content)
        {
            var chat = await _chatRepository.GetChat(chatId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found");

            var message = chat.SendMessage(sender, content);
            await _chatRepository.AddMessage(chatId, message);

            await _chatNotifier.NotifyMessageSent(message);
            return message;
        }
    }
}