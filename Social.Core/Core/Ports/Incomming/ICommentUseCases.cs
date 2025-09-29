namespace Social.Core.Ports.Incomming
{
    public interface ICommentUseCases
    {
        Task AddComment(Guid postId, Guid authorId, string? text, List<Image>? images);
        public Task VoteComment(Guid commentId, bool upVote, Guid userId);
        public Task<bool?> GetUserCommentVote(Guid postId, Guid commentId, Guid userId);
        public Task<bool?> UpdateCommentAsync(Guid commentId, Guid userId, string newContent);
        public Task<bool?> DeleteComment(Guid postId, Guid commentId, Guid userId);
    }
}
