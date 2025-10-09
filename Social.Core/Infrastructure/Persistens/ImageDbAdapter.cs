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

        public async Task AddAsync(Image image)
        {
            var entity = ImageMapper.ToEntity(image);
            await _context.Images.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid imageId)
        {
            var entity = await _context.Images.FindAsync(imageId);
            if (entity != null)
            {
                _context.Images.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<Image>> GetByChatMessageIdAsync(Guid chatMessageId)
        {
            var entity = await _context
                .Images.Where(i => i.ChatMessageId == chatMessageId)
                .ToListAsync();
            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        public async Task<IReadOnlyList<Image>> GetByCommentIdAsync(Guid commentId)
        {
            var entity = await _context.Images.Where(i => i.CommentId == commentId).ToListAsync();
            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        public async Task<Image?> GetByIdAsync(Guid imageId)
        {
            var entity = await _context.Images.FindAsync(imageId);
            return entity != null ? ImageMapper.ToDomain(entity) : null;
        }

        public async Task<IReadOnlyList<Image>> GetByPostIdAsync(Guid postId)
        {
            var entity = await _context.Images.Where(i => i.PostId == postId).ToListAsync();
            return entity.Select(ImageMapper.ToDomain).ToList();
        }

        public Task<Image> GetByProfileIdAsync(Guid profileId)
        {
            var entity = _context.Images.FirstOrDefault(i => i.ProfileId == profileId);
            return entity != null
                ? Task.FromResult(ImageMapper.ToDomain(entity))
                : Task.FromResult<Image>(null);
        }
    }

    public static class ImageMapper
    {
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

        public static Image ToDomain(this ImageEntity entity)
        {
            return new Image(entity.FileName, entity.ContentType, entity.Data) { Id = entity.Id };
        }
    }
}
