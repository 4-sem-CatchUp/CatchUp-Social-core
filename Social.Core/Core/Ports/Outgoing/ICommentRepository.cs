using Social.Core;

namespace Social.Core.Ports.Outgoing
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(Guid commentId);
        Task<IReadOnlyList<Comment>> GetByPostIdAsync(Guid postId);
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(Guid commentId);
    }
}
