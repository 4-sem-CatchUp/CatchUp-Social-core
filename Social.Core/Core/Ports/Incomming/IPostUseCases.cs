namespace Social.Core.Ports.Incomming
{
    public interface IPostUseCases
    {
        Task<Guid> CreatePostAsync(Guid authorId, string title, string? content, byte[]? image);
        public Task VotePost(Guid postId, bool upVote, Guid userId);
        public Task<bool?> GetUserPostVote(Guid postId, Guid userId);
        public Task UpdatePostAsync(
            Guid postId,
            string? newTitle,
            string? newContent,
            byte[]? image
        );
        public Task DeletePost(Guid postId);
    }
}
