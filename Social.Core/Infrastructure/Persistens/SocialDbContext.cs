using Microsoft.EntityFrameworkCore;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens.dbContexts
{
    public class SocialDbContext : DbContext
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<ProfileEntity> Profiles { get; set; }
        public DbSet<PostEntity> Posts { get; set; }
        public DbSet<CommentEntity> Comments { get; set; }
        public DbSet<ImageEntity> Images { get; set; }
        public DbSet<ChatEntity> Chats { get; set; }
        public DbSet<ChatMessageEntity> ChatMessages { get; set; }
        public DbSet<VoteEntity> Votes { get; set; }
        public DbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // Profile ↔ Friends (many-to-many)

            // -----------------------------
            modelBuilder
                .Entity<ProfileEntity>()
                .HasMany(p => p.Friends)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "Friendships",
                    j => j.HasOne<ProfileEntity>().WithMany().HasForeignKey("FriendId"),
                    j => j.HasOne<ProfileEntity>().WithMany().HasForeignKey("ProfileId"),
                    j =>
                    {
                        j.HasKey("ProfileId", "FriendId");
                        j.ToTable("Friendships");
                    }
                );

            // -----------------------------
            // Profile ↔ Subscriptions
            // -----------------------------
            modelBuilder
                .Entity<SubscriptionEntity>()
                .HasIndex(s => new { s.SubscriberId, s.PublisherId })
                .IsUnique();

            modelBuilder
                .Entity<SubscriptionEntity>()
                .HasOne(s => s.Subscriber)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<SubscriptionEntity>()
                .HasOne(s => s.Publisher)
                .WithMany()
                .HasForeignKey(s => s.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // Chat ↔ Profile (many-to-many)
            // -----------------------------
            modelBuilder
                .Entity<ChatParticipants>()
                .HasKey(cp => new { cp.ChatId, cp.ProfileId }); // Sammensat nøgle

            modelBuilder
                .Entity<ChatEntity>()
                .HasMany(c => c.Participants)
                .WithMany(p => p.Chats)
                .UsingEntity<ChatParticipants>();

            // -----------------------------
            // ChatMessage ↔ Chat
            // -----------------------------
            modelBuilder
                .Entity<ChatMessageEntity>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<ChatMessageEntity>()
                .HasOne(m => m.Sender)
                .WithMany(p => p.Messages) // Alternativt kan man have en Messages liste
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // Post ↔ Profile
            // -----------------------------
            modelBuilder
                .Entity<PostEntity>()
                .HasOne(p => p.Author)
                .WithMany(a => a.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------
            // Comment ↔ Post
            // -----------------------------
            modelBuilder
                .Entity<CommentEntity>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<CommentEntity>()
                .HasOne(c => c.Author)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------
            // Image ↔ Post / Comment / Profile / ChatMessage
            // -----------------------------

            modelBuilder
                .Entity<ImageEntity>()
                .HasOne(i => i.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder
                .Entity<ImageEntity>()
                .HasOne(i => i.Comment)
                .WithMany(c => c.Images)
                .HasForeignKey(i => i.CommentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder
                .Entity<ImageEntity>()
                .HasOne(i => i.Profile)
                .WithOne(p => p.ProfilePic)
                .HasForeignKey<ImageEntity>(i => i.ProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder
                .Entity<ImageEntity>()
                .HasOne(i => i.ChatMessage)
                .WithMany()
                .HasForeignKey(i => i.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------
            // Vote ↔ Profile / Post / Comment
            // -----------------------------
            modelBuilder
                .Entity<VoteEntity>()
                .HasOne(v => v.User)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
