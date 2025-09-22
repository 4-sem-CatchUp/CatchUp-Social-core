using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;
using Microsoft.AspNetCore.Http.Connections;
using Social.Core;

namespace Social.Core.Application
{
    public class PostService : IPostUseCases
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly ISubscribeUseCases _subscriptionService;
        /// <summary>
        /// Initializes a new instance of <see cref="PostService"/> with the required repositories and subscription use case.
        /// </summary>
        public PostService(IPostRepository postRepository,
            ICommentRepository commentRepository,
            IVoteRepository voteRepository,
            IProfileRepository profileRepository,
            ISubscribeUseCases subscribeUseCases)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
            _profileRepository = profileRepository;
            _subscriptionService = subscribeUseCases;
        }

        /// <summary>
        /// Creates and persists a new post authored by the specified user.
        /// </summary>
        /// <param name="authorId">The ID of the user who will own the post.</param>
        /// <param name="title">The post's title.</param>
        /// <param name="content">The post's content body.</param>
        /// <returns>The ID of the newly created post.</returns>
        public async Task<Guid> CreatePostAsync(Guid authorId, string title, string content)
        {
            var post = Post.CreateNewPost(authorId, title, content);
            await _postRepository.AddAsync(post);
            return post.Id;
        }


        public async Task VotePost(Guid postId, bool upVote, Guid userId)
        {
            var post = await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");

            var vote = post.AddVote(userId, upVote);

            if (vote.Action == VoteAction.Add)
                await _voteRepository.AddAsync(vote);
            else if (vote.Action == VoteAction.Update)
                await _voteRepository.UpdateAsync(vote);
            else if (vote.Action == VoteAction.Remove)
                await _voteRepository.DeleteAsync(vote.Id);

        }

        public async Task<bool?> GetUserPostVote(Guid postId, Guid userId)
        {
            var vote = await _voteRepository.GetUserVoteAsync(postId, VoteTargetType.Post, userId);
            return vote?.Upvote;
        }

        public async Task DeletePost(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");
            await _postRepository.DeleteAsync(postId);
        }

        public async Task UpdatePostAsync(Guid postId, string? newTitle, string? newContent)
        {
            var post = await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");
            post.UpdatePost(newTitle, newContent);
            await _postRepository.UpdateAsync(post);
        }
    }
}
