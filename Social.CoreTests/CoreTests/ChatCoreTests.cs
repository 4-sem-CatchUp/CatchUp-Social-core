using Social.Core;

namespace SocialCoreTests.CoreTests
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

        // --- BASIC CHAT ---
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

        [Test]
        public void AddParticipant_ShouldNotDuplicate()
        {
            _chat.AddParticipant(_user1); // already added
            Assert.That(_chat.Participants.Count, Is.EqualTo(2));
        }

        [Test]
        public void SendMessage_ShouldThrow_WhenSenderNotInChat()
        {
            var outsider = Profile.CreateNewProfile("Eve");
            Assert.Throws<InvalidOperationException>(() => _chat.SendMessage(outsider, "Hej"));
        }

        [Test]
        public void CloseChat_ShouldDeactivateChat()
        {
            _chat.CloseChat();
            Assert.That(_chat.Active, Is.False);
        }

        [Test]
        public void Chat_WithConstructorParticipants_ShouldBeActive()
        {
            var participants = new List<Profile> { _user1, _user2 };
            var chat2 = new Chat(participants);
            Assert.That(chat2.Active, Is.True);
            Assert.That(chat2.Participants.Count, Is.EqualTo(2));
        }
    }

    [TestFixture]
    public class ChatMessageTests
    {
        [Test]
        public void MarkAsSeen_ShouldSetSeenTrue()
        {
            var sender = Profile.CreateNewProfile("Alice");
            var msg = new ChatMessage(Guid.NewGuid(), sender, "Hej");
            msg.MarkAsSeen();

            Assert.That(msg.Seen, Is.True);
        }

        [Test]
        public void AddImage_ShouldStoreImage()
        {
            var sender = Profile.CreateNewProfile("Alice");
            var msg = new ChatMessage(Guid.NewGuid(), sender, "Hej");
            msg.AddImage("image.png", "image/png", new byte[] { 1, 2, 3 });

            Assert.That(msg.Image.FileName, Is.EqualTo("image.png"));
            Assert.That(msg.Image.ContentType, Is.EqualTo("image/png"));
            Assert.That(msg.Image.Data, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void EditContent_ShouldUpdateContentAndTimestamp()
        {
            var sender = Profile.CreateNewProfile("Alice");
            var msg = new ChatMessage(Guid.NewGuid(), sender, "Old content");

            var beforeEdit = msg.Timestamp;
            Thread.Sleep(10); // ensure timestamp changes
            msg.EditContent("New content");

            Assert.That(msg.Content, Is.EqualTo("New content"));
            Assert.That(msg.Timestamp, Is.GreaterThan(beforeEdit));
        }

        [Test]
        public void Seen_Default_ShouldBeFalse()
        {
            var sender = Profile.CreateNewProfile("Alice");
            var msg = new ChatMessage(Guid.NewGuid(), sender, "Hello");

            Assert.That(msg.Seen, Is.False);
        }
    }
}
