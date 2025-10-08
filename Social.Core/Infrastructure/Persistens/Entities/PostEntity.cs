using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class PostEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = "Nyt indlæg";
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid AuthorId { get; set; }
        public ProfileEntity Author { get; set; }

        // Navigation properties
        public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
        public ICollection<ImageEntity> Images { get; set; } = new List<ImageEntity>();
        public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();
    }
}
