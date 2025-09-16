using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class CommentServices : ICommentUseCases
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVoteRepository _voteRepository;
        public CommentServices(IPostRepository postRepository, ICommentRepository commentRepository, IVoteRepository voteRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
        }
        public async Task AddComment(Guid postId, Guid authorId, string text)
        {
            var post = await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");

            var comment = post.AddComment(authorId, text);
            await _commentRepository.AddAsync(comment);
        }

        public async Task VoteComment(Guid commentId, bool upVote, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId)
                ?? throw new InvalidOperationException("Comment not found");

            var vote = comment.AddVote(userId, upVote);

            if (vote.Action == VoteAction.Add)
                await _voteRepository.AddAsync(vote);
            else if (vote.Action == VoteAction.Update)
                await _voteRepository.UpdateAsync(vote);
            else if (vote.Action == VoteAction.Remove)
                await _voteRepository.DeleteAsync(vote.Id);
        }


        public async Task<bool?> GetUserCommentVote(Guid postId, Guid commentId, Guid userId)
        {
            var vote = await _voteRepository.GetUserVoteAsync(commentId, VoteTargetType.Comment, userId);
            return vote?.Upvote;
        }

        public async Task<bool?> DeleteComment(Guid postId, Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId)
                ?? throw new InvalidOperationException("Comment not found");

            if (comment.AuthorId != userId)
                throw new InvalidOperationException("User not authorized to delete this comment");

            await _commentRepository.DeleteAsync(commentId);
            return true;
        }

        public async Task<bool?> UpdateCommentAsync(Guid commentId, Guid userId, string newContent)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId)
                ?? throw new InvalidOperationException("Comment not found");

            if (comment.AuthorId != userId)
                throw new InvalidOperationException("User not authorized to update this comment");

            comment.UpdateComment(newContent);
            await _commentRepository.UpdateAsync(comment);
            return true;
        }
    }
}

