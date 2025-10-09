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

        public ImageDbAdapter(SocialDbContext context)
        {
            _context = context;
        }

        // Add a new image to the database
        public async Task AddAsync(Image image)
        {
            // Ensure the image does exist in the database
            var entity = ImageMapper.ToEntity(image);
            await _context.Images.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Delete an image by its ID
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

        // Get images by ChatMessageId
        public async Task<IReadOnlyList<Image>> GetByChatMessageIdAsync(Guid chatMessageId)
        {
            // Get all images associated with the specified ChatMessageId
            var entity = await _context
                .Images.Where(i => i.ChatMessageId == chatMessageId)
                .ToListAsync();

            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        // Get images by CommentId
        public async Task<IReadOnlyList<Image>> GetByCommentIdAsync(Guid commentId)
        {
            // Get all images associated with the specified CommentId
            var entity = await _context.Images.Where(i => i.CommentId == commentId).ToListAsync();

            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        // Get an image by its ID
        public async Task<Image?> GetByIdAsync(Guid imageId)
        {
            // Find the image entity by its ID
            var entity = await _context.Images.FindAsync(imageId);

            return entity != null ? ImageMapper.ToDomain(entity) : null;
        }

        // Get images by PostId
        public async Task<IReadOnlyList<Image>> GetByPostIdAsync(Guid postId)
        {
            // Get all images associated with the specified PostId
            var entity = await _context.Images.Where(i => i.PostId == postId).ToListAsync();

            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        // Get an image by ProfileId
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
        // Map from domain model to entity
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

        // Map from entity to domain model
        public static Image ToDomain(this ImageEntity entity)
        {
            return new Image(entity.FileName, entity.ContentType, entity.Data) { Id = entity.Id };
        }
    }
}
