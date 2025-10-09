using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class ChatDbAdapter : IChatRepository
    {
        private readonly SocialDbContext _context;

        public ChatDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        // Create a new chat
        public async Task CreateChat(Chat chat)
        {
            // Validate participants exist in the database
            var participants = await _context
                .Profiles.Where(p => chat.Participants.Select(cp => cp.Id).Contains(p.Id))
                .ToListAsync();

            // Ensure all participants exist
            if (participants.Count != chat.Participants.Count)
                throw new InvalidDataException("One or more participants do not exist.");

            // Map domain model to entity
            var entity = chat.ToEntity();
            entity.Participants = participants;

            // Add and save the new chat
            _context.Chats.Add(entity);
            await _context.SaveChangesAsync();
        }

        // Retrieve a chat by its ID
        public Task<Chat> GetChat(Guid chatId)
        {
            // Include participants and messages with senders
            var entity = _context
                .Chats.Include(c => c.Participants)
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .FirstOrDefault(c => c.ChatId == chatId);

            // If not found, throw an exception
            if (entity == null)
                throw new KeyNotFoundException("Chat not found.");

            // Map entity to domain model and return
            return Task.FromResult(entity.ToDomain());
        }

        // Update chat details (e.g., participants, active status)
        public async Task UpdateChat(Chat chat)
        {
            // Fetch existing chat with participants and messages
            var entity = _context
                .Chats.Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.ChatId == chat.ChatId);

            // If not found, throw an exception
            if (entity == null)
                throw new KeyNotFoundException("Chat not found.");

            // Update properties
            entity.Active = chat.Active;

            var participantsFromDb = await _context
                .Profiles.Where(p => chat.Participants.Select(cp => cp.Id).Contains(p.Id))
                .ToListAsync();

            // Add new participants
            foreach (var p in participantsFromDb)
            {
                if (!entity.Participants.Any(ep => ep.Id == p.Id))
                    entity.Participants.Add(p);
            }

            // Remove participants not in the updated chat
            var toRemove = entity
                .Participants.Where(ep => !chat.Participants.Any(cp => cp.Id == ep.Id))
                .ToList();

            foreach (var remove in toRemove)
            {
                entity.Participants.Remove(remove);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        // Add a new message to a chat
        public async Task AddMessage(Guid chatId, ChatMessage message)
        {
            var chat = await _context
                .Chats.Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ChatId == chatId);

            if (chat == null)
                throw new KeyNotFoundException("Chat not found.");

            if (!chat.Participants.Any(p => p.Id == message.Sender.Id))
                throw new InvalidOperationException("Sender is not a participant of the chat.");

            var messageEntity = message.ToEntity();

            _context.ChatMessages.Add(messageEntity);
            await _context.SaveChangesAsync();
        }

        // Delete a message from a chat
        public Task DeleteMessage(Guid chatId, Guid messageId)
        {
            // Find the message to delete
            var message = _context.ChatMessages.FirstOrDefault(m =>
                m.ChatId == chatId && m.MessageId == messageId
            );
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            // Find the chat to update its message list
            var chat = _context
                .Chats.Include(c => c.Messages)
                .FirstOrDefault(c => c.ChatId == chatId);
            if (chat == null)
                throw new KeyNotFoundException("Chat not found.");

            // Remove the message from the chat's message list
            chat.Messages.Remove(message);

            // Remove the message from the database
            _context.ChatMessages.Remove(message);

            return _context.SaveChangesAsync();
        }

        // Retrieve a specific message by its ID within a chat
        public Task<ChatMessage> GetMessage(Guid chatId, Guid messageId)
        {
            // Include sender and image details
            var message = _context
                .ChatMessages.Include(m => m.Sender)
                .Include(m => m.Image)
                .FirstOrDefault(m => m.ChatId == chatId && m.MessageId == messageId);

            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            return Task.FromResult(message.ToDomain());
        }

        // Retrieve a list of messages from a chat with pagination
        public async Task<List<ChatMessage>> GetMessages(Guid chatId, int count, int offset)
        {
            var entities = await _context
                .ChatMessages.Include(m => m.Sender)
                .Include(m => m.Image)
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.Timestamp)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return entities.Select(e => e.ToDomain()).ToList();
        }

        // Update an existing message's content or image
        public async Task UpdateMessage(Guid chatId, ChatMessage message)
        {
            var entity = await _context
                .ChatMessages.Include(m => m.Sender)
                .Include(m => m.Image)
                .FirstOrDefaultAsync(m => m.ChatId == chatId);
            if (entity == null)
                throw new KeyNotFoundException("Message not found.");

            // Update content and image
            entity.Content = message.Content ?? null;
            entity.Image = message.Image?.ToEntity() ?? null;

            await _context.SaveChangesAsync();
        }
    }

    public static class ChatMapper
    {
        public static ChatEntity ToEntity(this Chat chat)
        {
            return new ChatEntity
            {
                ChatId = chat.ChatId,
                Active = chat.Active,
                Participants = chat.Participants.Select(p => p.ToEntity()).ToList(),
                Messages = chat.Messages.Select(m => m.ToEntity()).ToList(),
            };
        }

        public static Chat ToDomain(this ChatEntity entity)
        {
            var chat = new Chat { ChatId = entity.ChatId, Active = entity.Active };

            foreach (var participant in entity.Participants)
            {
                chat.AddParticipant(participant.ToDomain());
            }

            foreach (var message in entity.Messages)
            {
                chat.Messages.Add(message.ToDomain());
            }
            return chat;
        }

        public static ChatMessageEntity ToEntity(this ChatMessage message)
        {
            return new ChatMessageEntity
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.Sender.Id,
                Content = message.Content,
                Timestamp = message.Timestamp,
                Seen = message.Seen,
                Image = message.Image?.ToEntity() ?? null,
            };
        }

        public static ChatMessage ToDomain(this ChatMessageEntity entity)
        {
            var message = new ChatMessage
            {
                MessageId = entity.MessageId,
                ChatId = entity.ChatId,
                Content = entity.Content,
                Timestamp = entity.Timestamp,
            };
            // Set Sender using reflection or internal setter
            typeof(ChatMessage).GetProperty("Sender")!.SetValue(message, entity.Sender.ToDomain());

            // Set Seen status
            if (entity.Seen)
            {
                message.MarkAsSeen();
            }

            // Set Image if exists
            if (entity.Image != null)
            {
                message.AddImage(entity.Image.ToDomain());
            }

            return message;
        }
    }
}
