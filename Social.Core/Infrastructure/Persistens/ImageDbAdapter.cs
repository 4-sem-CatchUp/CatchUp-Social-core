using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace Social.Infrastructure.Persistens
{
    public class ImageDbAdapter : IImageRepository
    {
        private readonly SocialDbContext _context;

        /// <summary>
        /// Initializes a new ImageDbAdapter that uses the provided SocialDbContext for data operations.
        /// </summary>
        public ImageDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists the provided domain Image as a new record in the database.
        /// </summary>
        /// <param name="image">The domain Image to add to the database.</param>
        public async Task AddAsync(Image image)
        {
            // Ensure the image does exist in the database
            var entity = ImageMapper.ToEntity(image);
            await _context.Images.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the image with the specified identifier from the database.
        /// </summary>
        /// <param name="imageId">The identifier of the image to delete.</param>
        /// <remarks>If no image with the given identifier exists, the operation has no effect.</remarks>
        public async Task DeleteAsync(Guid imageId)
        {
            // Find the image entity by its ID
            var entity = await _context.Images.FindAsync(imageId);

            // If found, remove it
            if (entity != null)
            {
                _context.Images.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieve all images associated with the specified chat message.
        /// </summary>
        /// <param name="chatMessageId">The identifier of the chat message whose images to retrieve.</param>
        /// <returns>A read-only list of images associated with the given chat message.</returns>
        public async Task<IReadOnlyList<Image>> GetByChatMessageIdAsync(Guid chatMessageId)
        {
            // Get all images associated with the specified ChatMessageId
            var entity = await _context
                .Images.Where(i => i.ChatMessageId == chatMessageId)
                .ToListAsync();

            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        /// <summary>
        /// Retrieves all images associated with the specified comment.
        /// </summary>
        /// <param name="commentId">The identifier of the comment whose images should be returned.</param>
        /// <returns>A read-only list of domain Image objects associated with the specified comment; empty if none are found.</returns>
        public async Task<IReadOnlyList<Image>> GetByCommentIdAsync(Guid commentId)
        {
            // Get all images associated with the specified CommentId
            var entity = await _context.Images.Where(i => i.CommentId == commentId).ToListAsync();

            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        /// <summary>
        /// Retrieves an image by its identifier.
        /// </summary>
        /// <param name="imageId">The identifier of the image to retrieve.</param>
        /// <returns>The matching <see cref="Image"/> if found, or <c>null</c> if no image has the specified identifier.</returns>
        public async Task<Image?> GetByIdAsync(Guid imageId)
        {
            // Find the image entity by its ID
            var entity = await _context.Images.FindAsync(imageId);

            return entity != null ? ImageMapper.ToDomain(entity) : null;
        }

        /// <summary>
        /// Retrieves all images associated with the specified post.
        /// </summary>
        /// <param name="postId">The post's unique identifier used to filter images.</param>
        /// <returns>A read-only list of Image domain objects for the post; empty list if none are found.</returns>
        public async Task<IReadOnlyList<Image>> GetByPostIdAsync(Guid postId)
        {
            // Get all images associated with the specified PostId
            var entity = await _context.Images.Where(i => i.PostId == postId).ToListAsync();

            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        /// <summary>
        /// Retrieves the first image associated with the specified profile identifier.
        /// </summary>
        /// <param name="profileId">The identifier of the profile whose image to retrieve.</param>
        /// <returns>The profile's Image if one exists, otherwise null.</returns>
        public Task<Image> GetByProfileIdAsync(Guid profileId)
        {
            // Get the image associated with the specified ProfileId
            var entity = _context.Images.FirstOrDefault(i => i.ProfileId == profileId);

            return entity != null
                ? Task.FromResult(ImageMapper.ToDomain(entity))
                : Task.FromResult<Image>(null);
        }
    }

    public static class ImageMapper
    {
        /// <summary>
        /// Creates an ImageEntity populated from the provided domain Image.
        /// </summary>
        /// <param name="image">The domain Image to map from.</param>
        /// <returns>An ImageEntity containing the corresponding values from the domain Image.</returns>
        public static ImageEntity ToEntity(this Image image)
        {
            return new ImageEntity
            {
                Id = image.Id,
                FileName = image.FileName,
                ContentType = image.ContentType,
                Data = image.Data,
            };
        }

        /// <summary>
        /// Convert a persistence ImageEntity into a domain Image.
        /// </summary>
        /// <param name="entity">The persistence entity to convert.</param>
        /// <returns>An Image domain object with property values copied from the entity, including the same Id.</returns>
        public static Image ToDomain(this ImageEntity entity)
        {
            return new Image(entity.FileName, entity.ContentType, entity.Data) { Id = entity.Id };
        }
    }
}