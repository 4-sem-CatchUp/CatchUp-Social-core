using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class CommentServices : ICommentUseCases
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly ISubscribeUseCases _subscriptionService;

        public CommentServices(
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IVoteRepository voteRepository,
            IProfileRepository profileRepository,
            ISubscribeUseCases subscriptionService
        )
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
            _profileRepository = profileRepository;
            _subscriptionService = subscriptionService;
        }

        public async Task AddComment(Guid postId, Guid authorId, string? text, List<Image>? images)
        {
            var post =
                await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");

            var comment = post.AddComment(authorId, text);
            if (images != null)
            {
                foreach (var image in images)
                {
                    comment.AddImage(image.FileName, image.ContentType, image.Data);
                }
            }
            await _commentRepository.AddAsync(comment);

            // Notify post author about the new comment
            var profile =
                await _profileRepository.GetProfileByIdAsync(post.AuthorId)
                ?? throw new InvalidOperationException("Profile not found");

            await _subscriptionService.Notify(
                profile,
                $"New comment ({comment.Id} ) on your post ( {postId} )"
            );
        }

        public async Task VoteComment(Guid commentId, bool upVote, Guid userId)
        {
            var comment =
                await _commentRepository.GetByIdAsync(commentId)
                ?? throw new InvalidOperationException("Comment not found");

            var vote = comment.AddVote(userId, upVote);

            if (vote.Action == VoteAction.Add)
                await _voteRepository.AddAsync(vote);
            else if (vote.Action == VoteAction.Update)
                await _voteRepository.UpdateAsync(vote);
            else if (vote.Action == VoteAction.Remove)
                await _voteRepository.DeleteAsync(vote.Id);

            // Notify comment author about the new vote
            var profile =
                await _profileRepository.GetProfileByIdAsync(comment.AuthorId)
                ?? throw new InvalidOperationException("Profile not found");
            await _subscriptionService.Notify(
                profile,
                $"Your comment ({comment.Id}) received a new vote."
            );
        }

        public async Task<bool?> GetUserCommentVote(Guid postId, Guid commentId, Guid userId)
        {
            var vote = await _voteRepository.GetUserVoteAsync(
                commentId,
                VoteTargetType.Comment,
                userId
            );
            return vote?.Upvote;
        }

        public async Task<bool?> DeleteComment(Guid postId, Guid commentId, Guid userId)
        {
            var comment =
                await _commentRepository.GetByIdAsync(commentId)
                ?? throw new InvalidOperationException("Comment not found");

            if (comment.AuthorId != userId)
                throw new InvalidOperationException("User not authorized to delete this comment");

            await _commentRepository.DeleteAsync(commentId);
            return true;
        }

        public async Task<bool?> UpdateCommentAsync(Guid commentId, Guid userId, string newContent)
        {
            var comment =
                await _commentRepository.GetByIdAsync(commentId)
                ?? throw new InvalidOperationException("Comment not found");

            if (comment.AuthorId != userId)
                throw new InvalidOperationException("User not authorized to update this comment");

            comment.UpdateComment(newContent);
            await _commentRepository.UpdateAsync(comment);
            return true;
        }
    }
}
