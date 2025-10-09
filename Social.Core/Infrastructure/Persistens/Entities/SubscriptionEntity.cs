using System.ComponentModel.DataAnnotations;

namespace Social.Infrastructure.Persistens.Entities
{
    public class SubscriptionEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SubscriberId { get; set; }
        public ProfileEntity Subscriber { get; set; }

        [Required]
        public Guid PublisherId { get; set; }
        public ProfileEntity Publisher { get; set; }

        public DateTime SubscribedOn { get; set; } = DateTime.UtcNow;
    }
}
