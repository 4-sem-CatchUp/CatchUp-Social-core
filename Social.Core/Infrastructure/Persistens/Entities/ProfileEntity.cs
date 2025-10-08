using System.ComponentModel.DataAnnotations;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens.Entities
{
    public class ProfileEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = "New User";
        public string Bio { get; set; } = string.Empty;
        public DateOnly DateOfSub { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        // Navigation properties
        public ICollection<PostEntity> Posts { get; set; } = new List<PostEntity>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();

        public Guid? ProfilePicId { get; set; }
        public ImageEntity? ProfilePic { get; set; }

        public ICollection<ProfileEntity> Friends { get; set; } = new List<ProfileEntity>();
    }
}
