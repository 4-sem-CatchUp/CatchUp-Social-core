namespace Social.Core.Ports.Incomming
{
    public interface IPostUseCases
    {
        Task<Guid> CreatePostAsync(Guid authorId, string title, string content);
        Task AddComment(Guid postId, Guid authorId, string text);
        public Task VotePost(Guid postId, bool upVote, Guid userId);
        public Task VoteComment(Guid commentId, bool upVote, Guid userId);
        public Task<bool?> GetUserPostVote(Guid postId, Guid userId);
        public Task<bool?> GetUserCommentVote(Guid postId, Guid commentId, Guid userId);
        public Task UpdatePostAsync(Guid postId, string? newTitle, string? newContent);
        public Task<bool?> UpdateCommentAsync(Guid postId, Guid commentId, Guid userId, string newContent);
        public Task DeletePost(Guid postId);
        public Task<bool?> DeleteComment(Guid postId, Guid commentId, Guid userId);

    }
}
