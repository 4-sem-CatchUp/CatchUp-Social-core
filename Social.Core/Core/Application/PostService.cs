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
        private readonly ISubscribeUseCases _subscriptionService;
        private readonly IProfileRepository _profileRepository;
        public PostService(IPostRepository postRepository, 
            ICommentRepository commentRepository, 
            IVoteRepository voteRepository,
            IProfileRepository profileRepository,
            ISubscribeUseCases subscribeUseCases)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
            _subscriptionService = subscribeUseCases;
            _profileRepository = profileRepository;

        }

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

            // Notify post author about the new vote
            var profile = await _profileRepository.GetProfileByIdAsync(post.AuthorId)
                ?? throw new InvalidOperationException("Profile not found");
            await _subscriptionService.Notify(profile, $"Your post ({post.Id}) received a new vote.");

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
