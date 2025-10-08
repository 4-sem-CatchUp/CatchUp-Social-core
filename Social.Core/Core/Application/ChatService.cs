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
