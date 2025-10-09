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

        /// <summary>
        /// Initializes a new CommentDbAdapter using the provided database context.
        /// </summary>
        /// <param name="context">The SocialDbContext used for comment persistence operations.</param>
        public CommentDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists a new domain Comment into the database.
        /// </summary>
        /// <param name="comment">The domain Comment to add and persist.</param>
        public async Task AddAsync(Comment comment)
        {
            var entity = comment.ToEntity();

            await _context.Comments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the comment with the specified identifier if it exists in the database.
        /// </summary>
        /// <param name="commentId">The identifier of the comment to delete.</param>
        public async Task DeleteAsync(Guid commentId)
        {
            var entity = await _context.Comments.FindAsync(commentId);

            if (entity != null)
            {
                _context.Comments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves a comment by its identifier including its votes and images.
        /// </summary>
        /// <returns>The found <see cref="Comment"/> populated with its votes and images, or `null` if no comment exists with the given identifier.</returns>
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

        /// <summary>
        /// Retrieves all comments for the specified post, including each comment's votes and images.
        /// </summary>
        /// <param name="postId">The identifier of the post whose comments should be returned.</param>
        /// <returns>A read-only list of domain <see cref="Comment"/> objects for the specified post, each populated with votes and images.</returns>
        public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(Guid postId)
        {
            var entity = await _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .Where(c => c.PostId == postId)
                .ToListAsync();

            return entity.Select(e => e.ToDomain()).ToList();
        }

        /// <summary>
        /// Updates the persisted comment matching the provided comment's Id with the provided content, images, and votes, then saves changes to the database.
        /// </summary>
        /// <param name="comment">The domain Comment containing the Id and new values to persist.</param>
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
        /// <summary>
        /// Converts a domain Comment into a persistence CommentEntity.
        /// </summary>
        /// <param name="comment">The domain Comment to convert.</param>
        /// <returns>A CommentEntity containing the same Id, author, timestamp, content (empty string if null), and collections of votes and images converted to their entity representations.</returns>
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

        /// <summary>
        /// Creates a domain Comment from a CommentEntity, including mapped votes and images.
        /// </summary>
        /// <param name="entity">The persistent CommentEntity to convert.</param>
        /// <returns>A Comment populated with the entity's Id, AuthorId, Content, Timestamp, Votes, and Images.</returns>
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