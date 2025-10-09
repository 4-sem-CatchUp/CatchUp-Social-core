using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Social.Core;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace SocialCoreTests.Infrastructure.Persistens
{
    [TestFixture]
    public class ChatDbAdapterTests
    {
        private SocialDbContext _context;
        private ChatDbAdapter _adapter;

        private Profile _userA;
        private Profile _userB;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new SocialDbContext(options);
            _adapter = new ChatDbAdapter(_context);

            _userA = new Profile("UserA");
            _userB = new Profile("UserB");
        }

        private async Task SeedProfilesAsync()
        {
            await _context.Profiles.AddRangeAsync(
                new ProfileEntity { Id = _userA.Id, Name = _userA.Name },
                new ProfileEntity { Id = _userB.Id, Name = _userB.Name }
            );
            await _context.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateChat_ShouldSaveChatWithParticipants()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA, _userB });

            await _adapter.CreateChat(chat);

            var saved = await _context
                .Chats.Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ChatId == chat.ChatId);

            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Participants.Count, Is.EqualTo(2));
            Assert.That(saved.Active, Is.True);
        }

        [Test]
        public void CreateChat_ShouldThrow_WhenParticipantDoesNotExist()
        {
            var chat = new Chat(new List<Profile> { _userA, _userB });

            var ex = Assert.ThrowsAsync<InvalidDataException>(async () =>
                await _adapter.CreateChat(chat)
            );
            Assert.That(ex.Message, Is.EqualTo("One or more participants do not exist."));
        }

        [Test]
        public async Task AddMessage_ShouldAddMessageToChat()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA, _userB });
            await _adapter.CreateChat(chat);

            var message = new ChatMessage(chat.ChatId, _userA, "Hej med dig!");
            await _adapter.AddMessage(chat.ChatId, message);

            var saved = await _context.ChatMessages.FirstOrDefaultAsync(m =>
                m.MessageId == message.MessageId
            );
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Content, Is.EqualTo("Hej med dig!"));
            Assert.That(saved.SenderId, Is.EqualTo(_userA.Id));
        }

        [Test]
        public async Task AddMessage_ShouldThrow_WhenSenderNotParticipant()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA });
            await _adapter.CreateChat(chat);

            var outsider = new Profile("Outsider");
            var message = new ChatMessage(chat.ChatId, outsider, "Jeg burde ikke kunne skrive her");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _adapter.AddMessage(chat.ChatId, message)
            );
            Assert.That(ex.Message, Is.EqualTo("Sender is not a participant of the chat."));
        }

        [Test]
        public async Task GetChat_ShouldReturnChatWithParticipants()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA, _userB });
            await _adapter.CreateChat(chat);

            var result = await _adapter.GetChat(chat.ChatId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Participants.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteMessage_ShouldRemoveMessage()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA, _userB });
            await _adapter.CreateChat(chat);

            var message = new ChatMessage(chat.ChatId, _userA, "Slet mig");
            await _adapter.AddMessage(chat.ChatId, message);

            await _adapter.DeleteMessage(chat.ChatId, message.MessageId);

            var deleted = await _context.ChatMessages.FirstOrDefaultAsync(m =>
                m.MessageId == message.MessageId
            );
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public async Task UpdateChat_ShouldAddAndRemoveParticipants()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA });
            await _adapter.CreateChat(chat);

            // Add userB to the chat
            chat.AddParticipant(_userB);
            await _adapter.UpdateChat(chat);

            var saved = await _context
                .Chats.Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ChatId == chat.ChatId);
            Assert.That(saved.Participants.Count, Is.EqualTo(2));

            // Remove userA
            chat.RemoveParticipant(_userA.Id);
            await _adapter.UpdateChat(chat);

            saved = await _context
                .Chats.Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ChatId == chat.ChatId);
            Assert.That(saved.Participants.Count, Is.EqualTo(1));
            Assert.That(saved.Participants.First().Id, Is.EqualTo(_userB.Id));
        }

        [Test]
        public async Task UpdateMessage_ShouldUpdateContent()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA });
            await _adapter.CreateChat(chat);

            var message = new ChatMessage(chat.ChatId, _userA, "Original besked");
            await _adapter.AddMessage(chat.ChatId, message);

            message.EditContent("Redigeret besked");
            await _adapter.UpdateMessage(chat.ChatId, message);

            var updated = await _context.ChatMessages.FirstAsync(m =>
                m.MessageId == message.MessageId
            );
            Assert.That(updated.Content, Is.EqualTo("Redigeret besked"));
        }

        [Test]
        public async Task GetMessage_ShouldReturnMessage()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA });
            await _adapter.CreateChat(chat);

            var message = new ChatMessage(chat.ChatId, _userA, "Test besked");
            await _adapter.AddMessage(chat.ChatId, message);

            var result = await _adapter.GetMessage(chat.ChatId, message.MessageId);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("Test besked"));
        }

        [Test]
        public async Task GetMessages_ShouldReturnMessagesWithPagination()
        {
            await SeedProfilesAsync();
            var chat = new Chat(new List<Profile> { _userA });
            await _adapter.CreateChat(chat);

            for (int i = 1; i <= 5; i++)
            {
                await _adapter.AddMessage(
                    chat.ChatId,
                    new ChatMessage(chat.ChatId, _userA, $"Besked {i}")
                );
            }

            var messages = await _adapter.GetMessages(chat.ChatId, 2, 1); // take 2, skip 1
            Assert.That(messages.Count, Is.EqualTo(2));
            Assert.That(messages[0].Content, Is.EqualTo("Besked 4"));
        }
    }
}
