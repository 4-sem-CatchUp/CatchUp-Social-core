﻿using Moq;
using NUnit.Framework;
using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialCoreTests
{
    [TestFixture]
    public class ChatServiceTests
    {
        private Mock<IChatRepository> _mockRepo;
        private Mock<IChatNotifier> _mockNotifier;
        private IChatUseCases _service;
        private Profile _user1;
        private Profile _user2;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IChatRepository>();
            _mockNotifier = new Mock<IChatNotifier>();
            _service = new ChatService(_mockRepo.Object, _mockNotifier.Object);

            _user1 = Profile.CreateNewProfile("Alice");
            _user2 = Profile.CreateNewProfile("Bob");
        }

        [Test]
        public async Task CreateChat_ShouldReturnChatWithParticipants()
        {
            var participants = new List<Profile> { _user1, _user2 };
            _mockRepo.Setup(r => r.CreateChat(It.IsAny<Chat>())).Returns(Task.CompletedTask);
            _mockNotifier.Setup(n => n.NotifyChatCreated(It.IsAny<Chat>())).Returns(Task.CompletedTask);

            var chat = await _service.CreateChat(_user1, participants);

            Assert.That(chat.Participants.Count, Is.EqualTo(2));
            _mockRepo.Verify(r => r.CreateChat(It.IsAny<Chat>()), Times.Once);
            _mockNotifier.Verify(n => n.NotifyChatCreated(It.IsAny<Chat>()), Times.Once);
        }

        [Test]
        public async Task SendMessage_ShouldAddMessageAndTriggerNotifier()
        {
            var chat = new Chat();
            chat.AddParticipant(_user1);

            _mockRepo.Setup(r => r.GetChat(It.IsAny<Guid>())).ReturnsAsync(chat);
            _mockRepo.Setup(r => r.AddMessage(It.IsAny<Guid>(), It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);
            _mockNotifier.Setup(n => n.NotifyMessageSent(It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);

            var msg = await _service.SendMessage(chat.ChatId, _user1, "Hej");

            Assert.That(msg.Content, Is.EqualTo("Hej"));
            _mockRepo.Verify(r => r.AddMessage(chat.ChatId, It.IsAny<ChatMessage>()), Times.Once);
            _mockNotifier.Verify(n => n.NotifyMessageSent(It.Is<ChatMessage>(m => m.Content == "Hej")), Times.Once);
        }

        [Test]
        public async Task DeleteMessage_ShouldCallRepository()
        {
            var message = new ChatMessage(Guid.NewGuid(), _user1, "Hello");
            _mockRepo.Setup(r => r.GetMessage(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(message);
            _mockRepo.Setup(r => r.DeleteMessage(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);

            await _service.DeleteMessage(Guid.NewGuid(), message.MessageId, _user1);

            _mockRepo.Verify(r => r.DeleteMessage(It.IsAny<Guid>(), message.MessageId), Times.Once);
        }

        [Test]
        public void DeleteMessage_ByAnotherUser_ShouldThrow()
        {
            var message = new ChatMessage(Guid.NewGuid(), _user1, "Hello");
            _mockRepo.Setup(r => r.GetMessage(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(message);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.DeleteMessage(Guid.NewGuid(), message.MessageId, _user2));
        }

        [Test]
        public async Task EditMessage_ShouldUpdateContent()
        {
            var message = new ChatMessage(Guid.NewGuid(), _user1, "Old");
            _mockRepo.Setup(r => r.GetMessage(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(message);
            _mockRepo.Setup(r => r.UpdateMessage(It.IsAny<Guid>(), It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);

            var edited = await _service.EditMessage(Guid.NewGuid(), message.MessageId, _user1, "New");

            Assert.That(edited.Content, Is.EqualTo("New"));
            _mockRepo.Verify(r => r.UpdateMessage(It.IsAny<Guid>(), message), Times.Once);
        }

        [Test]
        public async Task GetMessages_ShouldReturnList()
        {
            var chat = new Chat(new List<Profile> { _user1 });
            var messages = new List<ChatMessage>
            {
                new ChatMessage(chat.ChatId, _user1, "Hello"),
                new ChatMessage(chat.ChatId, _user1, "World")
            };

            _mockRepo.Setup(r => r.GetChat(It.IsAny<Guid>())).ReturnsAsync(chat);
            _mockRepo.Setup(r => r.GetMessages(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                     .ReturnsAsync(messages);

            var result = await _service.GetMessages(chat.ChatId, _user1, 10, 0);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Content, Is.EqualTo("Hello"));
        }
    }
}
