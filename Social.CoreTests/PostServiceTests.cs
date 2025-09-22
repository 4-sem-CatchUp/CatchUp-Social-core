using Moq;
using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace SocialCoreTests
{
    public class PostServiceTests
    {
        private Mock<IPostRepository> _mockPostRepo;
        private Mock<ICommentRepository> _mockCommentRepo;
        private Mock<IVoteRepository> _mockVoteRepo;
        private Mock<ISubscribeUseCases> _mockSubscriptionService;
        private Mock<IProfileRepository> _mockProfileRepo;

        private IPostUseCases _service;
        private ICommentUseCases _commentService;
        private Guid _postId;
        private Guid _userId;
        private Post _post;

        [SetUp]
        public void Setup()
        {
            _mockPostRepo = new Mock<IPostRepository>();
            _mockCommentRepo = new Mock<ICommentRepository>();
            _mockVoteRepo = new Mock<IVoteRepository>();
            _mockSubscriptionService = new Mock<ISubscribeUseCases>();
            _mockProfileRepo = new Mock<IProfileRepository>();

            _service = new PostService(
                _mockPostRepo.Object,
                _mockCommentRepo.Object,
                _mockVoteRepo.Object,
                _mockProfileRepo.Object,
                _mockSubscriptionService.Object
            );

            _commentService = new CommentServices(
                _mockPostRepo.Object,
                _mockCommentRepo.Object,
                _mockVoteRepo.Object,
                _mockProfileRepo.Object,
                _mockSubscriptionService.Object
            );

       
            _postId = Guid.NewGuid();
            _userId = Guid.NewGuid();
            _post = Post.CreateNewPost(_userId, "Title", "Content");

            _mockPostRepo.Setup(r => r.GetByIdAsync(_postId)).ReturnsAsync(_post);
            
            _mockProfileRepo.Setup(r => r.GetProfileByIdAsync(_userId))
                .ReturnsAsync(new Profile { Id = _userId, Name = "Test User" });
        }

        [Test]
        public async Task CreatePostAsync_ShouldAddPost()
        {
            Post? captured = null;
            _mockPostRepo
                .Setup(r => r.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => captured = p)
                .Returns(Task.CompletedTask);

            var postId = await _service.CreatePostAsync(_userId, "Title", "Content");

            Assert.IsNotNull(captured);
            Assert.That(captured.AuthorId, Is.EqualTo(_userId));
            Assert.That(captured.Title, Is.EqualTo("Title"));
            Assert.That(captured.Id, Is.EqualTo(postId));
            _mockPostRepo.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        }

        [Test]
        public async Task UpdatePost_ShouldUpdatePostAndCallRepository()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Old Title", "Old Content");
            _mockPostRepo.Setup(r => r.GetByIdAsync(_postId)).ReturnsAsync(post);

            await _service.UpdatePostAsync(_postId, "New Title", "New Content");

            Assert.That(post.Title, Is.EqualTo("New Title"));
            Assert.That(post.Content, Is.EqualTo("New Content"));
            _mockPostRepo.Verify(r => r.UpdateAsync(post), Times.Once);
        }

        [Test]
        public async Task DeletePost_ShouldCallRepository()
        {
            await _service.DeletePost(_postId);

            _mockPostRepo.Verify(r => r.DeleteAsync(_postId), Times.Once);
        }

        [Test]
        public async Task AddComment_ShouldCallCommentRepository()
        {
            var commentAuthor = Guid.NewGuid();
            await _commentService.AddComment(_postId, commentAuthor, "Nice!");

            _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Test]
        public async Task UpdateComment_ShouldUpdateCommentContent()
        {
            var comment = Comment.CreateNewComment(_userId, "Old Comment");
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _commentService.UpdateCommentAsync(comment.Id, _userId, "New Comment");

            Assert.That(comment.Content, Is.EqualTo("New Comment"));
            _mockCommentRepo.Verify(r => r.UpdateAsync(comment), Times.Once);
        }

        [Test]
        public async Task DeleteComment_ShouldCallRepository()
        {
            var comment = Comment.CreateNewComment(_userId, "To Delete");
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _commentService.DeleteComment(_postId, comment.Id, _userId);

            _mockCommentRepo.Verify(r => r.DeleteAsync(comment.Id), Times.Once);
        }

        [Test]
        public async Task VotePost_ShouldCallVoteRepository()
        {
            await _service.VotePost(_postId, true, _userId);

            _mockVoteRepo.Verify(r => r.AddAsync(It.IsAny<Vote>()), Times.Once);
        }

        [Test]
        public async Task GetUserPostVote_ShouldReturnVote()
        {
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                TargetId = _postId,
                UserId = _userId,
                Upvote = true,
                VoteTargetType = VoteTargetType.Post,
            };

            _mockVoteRepo
                .Setup(r => r.GetUserVoteAsync(_postId, VoteTargetType.Post, _userId))
                .ReturnsAsync(vote);

            var result = await _service.GetUserPostVote(_postId, _userId);

            Assert.IsTrue(result.Value);
        }
    }
}
