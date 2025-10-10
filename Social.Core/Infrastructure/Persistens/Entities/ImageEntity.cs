using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class ImageEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public byte[] Data { get; set; } = Array.Empty<byte>();

        // Navigation properties
        public Guid? PostId { get; set; }
        public PostEntity? Post { get; set; }
        public Guid? CommentId { get; set; }
        public CommentEntity? Comment { get; set; }
        public Guid? ProfileId { get; set; }
        public ProfileEntity? Profile { get; set; }
        public Guid? ChatMessageId { get; set; }
        public ChatMessageEntity? ChatMessage { get; set; }
    }
}
