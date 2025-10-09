using Microsoft.AspNetCore.Http.Connections;
using Social.Core;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class PostService : IPostUseCases
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly ISubscribeUseCases _subscriptionService;

        public PostService(
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            IVoteRepository voteRepository,
            IProfileRepository profileRepository,
            ISubscribeUseCases subscribeUseCases
        )
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _voteRepository = voteRepository;
            _profileRepository = profileRepository;
            _subscriptionService = subscribeUseCases;
        }

        /// <summary>
        /// Creates a new post for the specified author with a title, optional content, and optional images.
        /// </summary>
        /// <param name="authorId">The identifier of the post author.</param>
        /// <param name="title">The post title.</param>
        /// <param name="content">Optional post body text; pass null to create a post without content.</param>
        /// <param name="images">Optional list of images to attach; each item provides filename, content type, and binary data.</param>
        /// <returns>The identifier of the newly created post.</returns>
        public async Task<Guid> CreatePostAsync(
            Guid authorId,
            string title,
            string? content,
            List<Image>? images
        )
        {
            var post = Post.CreateNewPost(authorId, title, content);
            if (images != null)
            {
                foreach (var image in images)
                {
                    post.AddImage(image.FileName, image.ContentType, image.Data);
                }
            }
            await _postRepository.AddAsync(post);
            return post.Id;
        }

        public async Task VotePost(Guid postId, bool upVote, Guid userId)
        {
            var post =
                await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");

            var vote = post.AddVote(userId, upVote);

            if (vote.Action == VoteAction.Add)
                await _voteRepository.AddAsync(vote);
            else if (vote.Action == VoteAction.Update)
                await _voteRepository.UpdateAsync(vote);
            else if (vote.Action == VoteAction.Remove)
                await _voteRepository.DeleteAsync(vote.Id);

            // Notify post author about the new vote
            var profile =
                await _profileRepository.GetProfileByIdAsync(post.AuthorId)
                ?? throw new InvalidOperationException("Profile not found");
            await _subscriptionService.Notify(
                profile,
                $"Your post ({post.Id}) received a new vote."
            );
        }

        public async Task<bool?> GetUserPostVote(Guid postId, Guid userId)
        {
            var vote = await _voteRepository.GetUserVoteAsync(postId, VoteTargetType.Post, userId);
            return vote?.Upvote;
        }

        /// <summary>
        /// Deletes the post identified by the given id.
        /// </summary>
        /// <param name="postId">The identifier of the post to delete.</param>
        /// <exception cref="InvalidOperationException">Thrown if no post with the specified id exists.</exception>
        public async Task DeletePost(Guid postId)
        {
            var post =
                await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");
            await _postRepository.DeleteAsync(postId);
        }

        /// <summary>
        /// Updates the title and/or content of an existing post.
        /// </summary>
        /// <param name="postId">Identifier of the post to update.</param>
        /// <param name="newTitle">New title to set; pass <c>null</c> to leave the title unchanged.</param>
        /// <param name="newContent">New content to set; pass <c>null</c> to leave the content unchanged.</param>
        /// <exception cref="InvalidOperationException">Thrown if a post with the specified <paramref name="postId"/> does not exist.</exception>
        public async Task UpdatePostAsync(Guid postId, string? newTitle, string? newContent)
        {
            var post =
                await _postRepository.GetByIdAsync(postId)
                ?? throw new InvalidOperationException("Post not found");
            post.UpdatePost(newTitle, newContent);
            await _postRepository.UpdateAsync(post);
        }
    }
}