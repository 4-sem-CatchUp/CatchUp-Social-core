using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class ChatMessageEntity
    {
        [Key]
        public Guid MessageId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ChatId { get; set; }
        public ChatEntity Chat { get; set; }

        [Required]
        public Guid SenderId { get; set; }
        public ProfileEntity Sender { get; set; }

        [Required]
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Seen { get; set; }
        public ImageEntity? Image { get; set; }
    }
}
