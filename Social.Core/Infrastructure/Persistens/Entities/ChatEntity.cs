using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class Chat
    {
        [Key]
        public Guid ChatId { get; set; } = Guid.NewGuid();
        public bool Active { get; set; } = false;

        public ICollection<ProfileEntity> Participants { get; set; } = new List<ProfileEntity>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
