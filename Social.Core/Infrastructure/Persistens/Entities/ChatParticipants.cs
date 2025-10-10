using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class ChatParticipants
    {
        public Guid ChatId { get; set; }
        public ChatEntity Chat { get; set; }

        public Guid ProfileId { get; set; }
        public ProfileEntity Profile { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "Member"; // f.eks. "Admin", "Moderator"
    }
}
