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

        /// <summary>
        /// Initializes a new instance of <see cref="PostDbAdapter"/> using the provided <see cref="SocialDbContext"/>.
        /// </summary>
        public PostDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists the given Post aggregate to the database.
        /// </summary>
        /// <param name="post">The domain Post to persist; it will be converted to a persistence entity and saved.</param>
        public async Task AddAsync(Post post)
        {
            // Ensure the user exists in the database
            var entity = post.ToEntity();

            await _context.Posts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the post with the specified identifier from the database.
        /// </summary>
        /// <param name="postId">Identifier of the post to remove; no action is taken if no matching post exists.</param>
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

        /// <summary>
        /// Retrieves all posts from the database including their comments, votes, and images.
        /// </summary>
        /// <returns>A collection of domain <see cref="Post"/> instances with their related Comments, Votes, and Images populated.</returns>
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

        /// <summary>
        /// Retrieve a Post aggregate by its ID including comments, images, and votes.
        /// </summary>
        /// <returns>The domain Post with related comments, images, and votes, or <c>null</c> if no post with the given ID exists.</returns>
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

        /// <summary>
        /// Updates an existing post and synchronizes its comments, images, and votes with the provided domain post.
        /// </summary>
        /// <param name="post">The domain post containing updated scalar fields and collections to persist; if no matching persisted post exists, the call is a no-op.</param>
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
        /// <summary>
        /// Converts a domain Post into its persistence PostEntity representation.
        /// </summary>
        /// <param name="post">The domain Post to convert.</param>
        /// <returns>A PostEntity with scalar fields copied and with Comments, Votes, and Images converted; if any domain collections are null, the corresponding entity collections will be empty.</returns>
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

        /// <summary>
        /// Converts a persistence PostEntity into a fully populated domain Post.
        /// </summary>
        /// <param name="entity">The persistence entity to convert.</param>
        /// <returns>A domain Post populated with Title, Content, AuthorId, CreatedAt and with Votes, Comments, and Images attached. Nested items include their IDs and timestamps where applicable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
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