using Social.Core.Ports.Outgoing;
using Social.Core.Ports.Incomming;

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

        public Task CreateChat(Profile creator, List<Profile> participants)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMessage(Guid chatId, Guid messageId, Profile deleter)
        {
            throw new NotImplementedException();
        }

        public Task EditMessage(Guid chatId, Guid messageId, Profile editor, string newContent)
        {
            throw new NotImplementedException();
        }

        public Task GetMessages(Guid chatId, Profile requester, int count, int offset)
        {
            throw new NotImplementedException();
        }

        public Task SendMessage(Guid chatId, Profile sender, string content)
        {
            throw new NotImplementedException();
        }
    }
}
