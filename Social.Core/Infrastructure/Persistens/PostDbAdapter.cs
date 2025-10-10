using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class PostDbAdapter : IPostRepository
    {
        private readonly SocialDbContext _context;

        public PostDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        // Add a new post to the database
        public async Task AddAsync(Post post)
        {
            // Ensure the user exists in the database
            var entity = post.ToEntity();

            await _context.Posts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Delete a post by its ID
        public async Task DeleteAsync(Guid postId)
        {
            // Find the post entity by its ID
            var entity = await _context.Posts.FindAsync(postId);

            // If found, remove it
            if (entity != null)
            {
                _context.Posts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Get all posts along with their comments, images, and votes
        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            // Get all posts with related comments, votes, and images
            var entitys = await _context
                .Posts.Include(p => p.Comments)
                .Include(p => p.Votes)
                .Include(p => p.Images)
                .ToListAsync();

            return entitys.Select(e => e.ToDomain()).ToList();
        }

        // Get a post by its ID along with its comments, images, and votes
        public async Task<Post?> GetByIdAsync(Guid postId)
        {
            // Get the post with related comments and images
            var entity = await _context
                .Posts.Include(p => p.Comments)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (entity == null)
                return null;

            // Load votes separately
            var votes = await _context
                .Votes.Where(v =>
                    v.TargetId == postId && v.VoteTargetType == Entities.VoteTargetType.Post
                )
                .ToListAsync();

            entity.Votes = votes;

            return entity?.ToDomain();
        }

        // Update a post along with its comments, images, and votes
        public async Task UpdateAsync(Post post)
        {
            // Fetch the existing post with related comments and images
            var entity = await _context
                .Posts.Include(p => p.Comments)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == post.Id);
            if (entity == null)
                return;

            entity.Title = post.Title;
            entity.Content = post.Content;

            // Add or update comments
            foreach (var comment in post.Comments)
            {
                var existing = entity.Comments.FirstOrDefault(c => c.Id == comment.Id);
                if (existing != null)
                {
                    existing.Content = comment.Content;
                }
                else
                {
                    var newComment = await _context.Comments.FindAsync(comment.Id);
                    newComment.PostId = post.Id;
                    entity.Comments.Add(newComment);
                }
            }

            // Add or update images
            foreach (var image in post.Images)
            {
                var existing = entity.Images.FirstOrDefault(i => i.Id == image.Id);
                if (existing != null)
                {
                    existing.FileName = image.FileName;
                    existing.ContentType = image.ContentType;
                    existing.Data = image.Data;
                }
                else
                {
                    var newImage = image.ToEntity();
                    newImage.PostId = post.Id;
                    entity.Images.Add(newImage);
                }
            }

            // Add or update votes
            var votes = await _context
                .Votes.Where(v =>
                    v.TargetId == post.Id && v.VoteTargetType == Entities.VoteTargetType.Post
                )
                .ToListAsync();

            foreach (var vote in post.Votes)
            {
                // Check if the vote already exists
                var existing = votes.FirstOrDefault(v => v.Id == vote.Id);
                if (existing != null)
                {
                    existing.Upvote = vote.Upvote;
                }
                else
                {
                    var newVote = vote.ToEntity();
                    newVote.TargetId = post.Id;
                    newVote.VoteTargetType = Entities.VoteTargetType.Post;
                    _context.Votes.Add(newVote);
                }
            }
            await _context.SaveChangesAsync();
        }
    }

    public static class PostMapper
    {
        // Map from Domain to Entity
        public static PostEntity ToEntity(this Post post)
        {
            return new PostEntity
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                Comments =
                    post.Comments?.Select(c => c.ToEntity()).ToList() ?? new List<CommentEntity>(),
                Votes = post.Votes?.Select(v => v.ToEntity()).ToList() ?? new List<VoteEntity>(),
                Images = post.Images?.Select(i => i.ToEntity()).ToList() ?? new List<ImageEntity>(),
            };
        }

        // Map from Entity to Domain
        public static Post ToDomain(this PostEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var post = new Post
            {
                Title = entity.Title,
                Content = entity.Content,
                AuthorId = entity.AuthorId,
                CreatedAt = entity.CreatedAt,
            };

            // Sæt Id via reflection (eller internal setter)
            typeof(Post).GetProperty("Id")!.SetValue(post, entity.Id);

            // Votes
            foreach (var voteEntity in entity.Votes ?? Enumerable.Empty<VoteEntity>())
            {
                var vote = voteEntity.ToDomain();
                vote.TargetId = entity.Id;
                post.AddVote(vote);
            }

            // Comments
            foreach (var commentEntity in entity.Comments ?? Enumerable.Empty<CommentEntity>())
            {
                var comment = commentEntity.ToDomain();
                typeof(Comment).GetProperty("Id")!.SetValue(comment, commentEntity.Id);
                typeof(Comment)
                    .GetProperty("Timestamp")!
                    .SetValue(comment, commentEntity.Timestamp);
                post.AddComment(comment);
            }

            // Images
            foreach (var imageEntity in entity.Images ?? Enumerable.Empty<ImageEntity>())
            {
                var image = imageEntity.ToDomain();
                typeof(Image).GetProperty("Id")!.SetValue(image, imageEntity.Id);
                post.AddImage(image);
            }

            return post;
        }
    }
}
