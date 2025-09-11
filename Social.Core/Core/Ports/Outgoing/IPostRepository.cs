using Social.Core;

namespace Social.Core.Ports.Outgoing
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(Guid postId);
        Task<IEnumerable<Post>> GetAllAsync();
        Task AddAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Guid postId);
    }
}
