using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialCoreTests
{
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
    using Moq;
    using Social.Core;
    using Social.Core.Application;
    using Social.Core.Ports.Outgoing;

    [TestFixture]
    public class ChatServiceTests
    {
        private Mock<IChatRepository> _mockRepo;
        private IChatNotifier _notifier;
        private ChatService _service;
        private Profile _user1;
        private Profile _user2;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IChatRepository>();
            _service = new ChatService(_mockRepo.Object, _notifier);
            _user1 = Profile.CreateNewProfile("Alice");
            _user2 = Profile.CreateNewProfile("Bob");
        }

        //[Test]
        //public void CreateChat_ShouldReturnChatWithParticipants()
        //{
        //    var participants = new List<Profile> { _user1, _user2 };
        //    _mockRepo.Setup(r => r.CreateChat(It.IsAny<Chat>()))
        //             .Returns((Chat c) => c);

        //    var chat = _service.CreateChat(_user1, participants);

        //    Assert.That(chat.Participants.Count, Is.EqualTo(2));
        //    _mockRepo.Verify(r => r.CreateChat(It.IsAny<Chat>()), Times.Once);
        //}

        //[Test]
        //public void SendMessage_ShouldAddMessage()
        //{
        //    var chat = new Chat();
        //    chat.AddParticipant(_user1);
        //    _mockRepo.Setup(r => r.GetChat(It.IsAny<Guid>())).Returns(chat);
        //    _mockRepo.Setup(r => r.UpdateChat(It.IsAny<Chat>()));

        //    var msg = _service.SendMessage(chat.ChatId, _user1, "Hej");

        //    Assert.That(msg.Content, Is.EqualTo("Hej"));
        //    _mockRepo.Verify(r => r.UpdateChat(chat), Times.Once);
        //}

        //[Test]
        //public async Task SendMessage_ShouldTriggerNotifier()
        //{
        //    var repo = new Mock<IChatRepository>();
        //    var notifier = new Mock<IChatNotifier>();

        //    var service = new ChatService(repo.Object, notifier.Object);

        //    var chat = new Chat();
        //    var sender = Profile.CreateNewProfile("Alice");
        //    chat.AddParticipant(sender);

        //    repo.Setup(r => r.GetChat(It.IsAny<Guid>())).Returns(chat);

        //    var msg = service.SendMessage(chat.ChatId, sender, "Hej");

        //    notifier.Verify(n => n.NotifyMessageSent(It.Is<Message>(m => m.Content == "Hej")), Times.Once);
        //}

    }

}
