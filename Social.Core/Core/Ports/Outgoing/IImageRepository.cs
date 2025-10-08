namespace Social.Core.Ports.Outgoing
{
    public interface IImageRepository
    {
        Task AddAsync(Image image);
        Task DeleteAsync(Guid imageId);
        Task<Image?> GetByIdAsync(Guid imageId);

        Task<IReadOnlyList<Image>> GetByCommentIdAsync(Guid commentId);
        Task<IReadOnlyList<Image>> GetByPostIdAsync(Guid postId);
        Task<Image> GetByProfileIdAsync(Guid profileId);
        Task<IReadOnlyList<Image>> GetByChatMessageIdAsync(Guid chatMessageId);
    }
}
