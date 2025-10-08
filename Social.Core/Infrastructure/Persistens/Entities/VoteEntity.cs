using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class VoteEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TargetId { get; set; } // Kan pege på Post eller Comment

        public VoteTargetType VoteTargetType { get; set; } // Angiver om det er Post eller Comment

        [Required]
        public Guid UserId { get; set; }
        public ProfileEntity User { get; set; }

        public bool Upvote { get; set; }
        public VoteAction Action { get; set; } = VoteAction.Add;
    }

    public enum VoteTargetType
    {
        Post,
        Comment,
    }

    public enum VoteAction
    {
        Add,
        Remove,
        Update,
    }
}
