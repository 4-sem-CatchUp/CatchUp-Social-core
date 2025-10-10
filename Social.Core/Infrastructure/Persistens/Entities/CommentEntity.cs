using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class CommentEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AuthorId { get; set; }
        public ProfileEntity Author { get; set; }

        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();
        public ICollection<ImageEntity> Images { get; set; } = new List<ImageEntity>();

        public Guid? PostId { get; set; }
        public PostEntity? Post { get; set; }
    }
}
