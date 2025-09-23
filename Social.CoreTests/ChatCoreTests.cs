using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Social.Core;

namespace SocialCoreTests
{
    [TestFixture]
    public class ChatCoreTests
    {
        private Profile _user1;
        private Profile _user2;
        private Chat _chat;

        [SetUp]
        public void Setup()
        {
            _user1 = Profile.CreateNewProfile("Alice");
            _user2 = Profile.CreateNewProfile("Bob");
            _chat = new Chat();
            _chat.AddParticipant(_user1);
            _chat.AddParticipant(_user2);
        }

        [Test]
        public void SendMessage_ShouldAddMessage()
        {
            var msg = _chat.SendMessage(_user1, "Hej Bob");

            Assert.That(_chat.Messages.Count, Is.EqualTo(1));
            Assert.That(msg.Content, Is.EqualTo("Hej Bob"));
            Assert.That(msg.Sender, Is.EqualTo(_user1));
        }

        [Test]
        public void RemoveParticipant_ShouldDecreaseCount()
        {
            _chat.RemoveParticipant(_user2.Id);
            Assert.That(_chat.Participants.Count, Is.EqualTo(1));
        }
    }

    [TestFixture]
    public class MessageCoreTests
    {
        [Test]
        public void MarkAsSeen_ShouldSetSeenTrue()
        {
            var sender = Profile.CreateNewProfile("Alice");
            var msg = new ChatMessage(Guid.NewGuid(), sender, "Hej");
            msg.MarkAsSeen();

            Assert.That(msg.Seen, Is.True);
        }
    }
}
