using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class CommentDbAdapter : ICommentRepository
    {
        private readonly SocialDbContext _context;

        public CommentDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        // Add a new comment to the database
        public async Task AddAsync(Comment comment)
        {
            var entity = comment.ToEntity();

            await _context.Comments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Delete a comment by its ID
        public async Task DeleteAsync(Guid commentId)
        {
            var entity = await _context.Comments.FindAsync(commentId);

            if (entity != null)
            {
                _context.Comments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Get a comment by its ID along with its votes and images
        public Task<Comment?> GetByIdAsync(Guid commentId)
        {
            // Get the comment with related votes and images
            var entity = _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == commentId);

            return entity?.ToDomain() is Comment comment
                ? Task.FromResult<Comment?>(comment)
                : Task.FromResult<Comment?>(null);
        }

        // Get all comments for a specific post along with their votes and images
        public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(Guid postId)
        {
            var entity = await _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .Where(c => c.PostId == postId)
                .ToListAsync();

            return entity.Select(e => e.ToDomain()).ToList();
        }

        // Update an existing comment
        public async Task UpdateAsync(Comment comment)
        {
            var entity = await _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            // If the comment exists, update its properties
            if (entity != null)
            {
                entity.Content = comment.Content ?? string.Empty;
                entity.Images = comment.Images.Select(i => i.ToEntity()).ToList();
                entity.Votes = comment.Votes.Select(v => v.ToEntity()).ToList();
                _context.Comments.Update(entity);
            }

            await _context.SaveChangesAsync();
        }
    }

    public static class CommentMapper
    {
        // Map from Domain to Entity
        public static CommentEntity ToEntity(this Comment comment)
        {
            return new CommentEntity
            {
                Id = comment.Id,
                AuthorId = comment.AuthorId,
                Content = comment.Content ?? string.Empty,
                Timestamp = comment.Timestamp,
                Votes = comment.Votes.Select(v => v.ToEntity()).ToList(),
                Images = comment.Images.Select(i => i.ToEntity()).ToList(),
            };
        }

        // Map from Entity to Domain
        public static Comment ToDomain(this CommentEntity entity)
        {
            var comment = new Comment(
                entity.AuthorId,
                entity.Content ?? string.Empty,
                entity.Timestamp,
                entity.Votes?.Select(v => v.ToDomain()).ToList() ?? new List<Vote>()
            );

            typeof(Comment).GetProperty("Id")!.SetValue(comment, entity.Id);

            foreach (var imageEntity in entity.Images)
            {
                comment.AddImage(imageEntity.ToDomain());
            }

            return comment;
        }
    }
}
