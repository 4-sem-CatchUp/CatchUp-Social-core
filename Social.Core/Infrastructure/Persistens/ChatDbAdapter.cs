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

        /// <summary>
        /// Initializes a new instance of <see cref="ChatDbAdapter"/> using the provided <see cref="SocialDbContext"/> for data access.
        /// </summary>
        public ChatDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a chat from the provided domain model and persists it to the database.
        /// </summary>
        /// <param name="chat">The domain Chat to create. Must include participant entries with valid profile IDs.</param>
        /// <exception cref="System.IO.InvalidDataException">Thrown when one or more participants do not exist in the database.</exception>
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

        /// <summary>
        /// Retrieves the chat identified by the provided chatId, including its participants and messages.
        /// </summary>
        /// <param name="chatId">Unique identifier of the chat to retrieve.</param>
        /// <returns>The chat domain object populated with participants and messages.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no chat with the specified id exists.</exception>
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

        /// <summary>
        /// Updates an existing chat's active state and reconciles its participant list in the database.
        /// </summary>
        /// <param name="chat">Domain chat containing the ChatId to identify the persisted chat and the updated Active flag and Participants collection.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no chat with the given ChatId exists.</exception>
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

        /// <summary>
        /// Adds a new message to the specified chat after validating the chat exists and the sender is a participant.
        /// </summary>
        /// <param name="chatId">The identifier of the chat to add the message to.</param>
        /// <param name="message">The chat message to add.</param>
        /// <exception cref="KeyNotFoundException">Thrown when no chat with the given <paramref name="chatId"/> exists.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the message sender is not a participant of the chat.</exception>
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

        /// <summary>
        /// Delete the specified message from the given chat and persist the removal.
        /// </summary>
        /// <param name="chatId">Identifier of the chat containing the message.</param>
        /// <param name="messageId">Identifier of the message to delete.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the message does not exist.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the chat does not exist.</exception>
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

        /// <summary>
        /// Retrieves a chat message by its identifier within the specified chat.
        /// </summary>
        /// <param name="chatId">The identifier of the chat containing the message.</param>
        /// <param name="messageId">The identifier of the message to retrieve.</param>
        /// <returns>The requested ChatMessage mapped to the domain model.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the message is not found in the specified chat.</exception>
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

        /// <summary>
        /// Retrieves a page of messages for the specified chat, ordered from newest to oldest.
        /// </summary>
        /// <param name="chatId">The identifier of the chat whose messages are requested.</param>
        /// <param name="count">The maximum number of messages to return.</param>
        /// <param name="offset">The number of most-recent messages to skip (for pagination).</param>
        /// <returns>A list of ChatMessage objects for the specified chat ordered by timestamp descending.</returns>
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

        /// <summary>
        /// Updates the content and image of the first message found in the specified chat.
        /// </summary>
        /// <param name="chatId">The identifier of the chat containing the message to update.</param>
        /// <param name="message">A ChatMessage whose Content and Image will replace the stored values on the found message.</param>
        /// <exception cref="KeyNotFoundException">Thrown when no message exists for the given chat.</exception>
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
        /// <summary>
        /// Converts a domain Chat into a ChatEntity suitable for persistence, including mapped participants and messages.
        /// </summary>
        /// <returns>A ChatEntity representing the provided domain Chat, with Participants and Messages converted to their entity types.</returns>
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

        /// <summary>
        /// Converts a persistence ChatEntity into a domain Chat and populates its participants and messages.
        /// </summary>
        /// <param name="entity">The ChatEntity to convert.</param>
        /// <returns>A domain Chat with ChatId, Active flag, Participants, and Messages copied from the entity.</returns>
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

        /// <summary>
        /// Converts a domain ChatMessage into its persistence-layer ChatMessageEntity representation.
        /// </summary>
        /// <param name="message">The domain chat message to convert.</param>
        /// <returns>A ChatMessageEntity with properties mapped from the provided domain message.</returns>
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

        /// <summary>
        /// Converts a persistence ChatMessageEntity into its domain ChatMessage representation.
        /// </summary>
        /// <returns>The domain ChatMessage populated with MessageId, ChatId, Content, Timestamp, Sender, Seen state, and optional Image.</returns>
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