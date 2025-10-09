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

        public async Task AddAsync(Comment comment)
        {
            var entity = comment.ToEntity();
            await _context.Comments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid commentId)
        {
            var entity = await _context.Comments.FindAsync(commentId);
            if (entity != null)
            {
                _context.Comments.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public Task<Comment?> GetByIdAsync(Guid commentId)
        {
            var entity = _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == commentId);

            return entity?.ToDomain() is Comment comment
                ? Task.FromResult<Comment?>(comment)
                : Task.FromResult<Comment?>(null);
        }

        public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(Guid postId)
        {
            var entity = await _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .Where(c => c.PostId == postId)
                .ToListAsync();

            return entity.Select(e => e.ToDomain()).ToList();
        }

        public async Task UpdateAsync(Comment comment)
        {
            var entity = await _context
                .Comments.Include(c => c.Votes)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

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
