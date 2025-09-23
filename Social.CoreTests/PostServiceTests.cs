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
            _post = Post.CreateNewPost(_userId, "Title", "Content", null);

            _mockPostRepo.Setup(r => r.GetByIdAsync(_postId)).ReturnsAsync(_post);
            _mockProfileRepo
                .Setup(r => r.GetProfileByIdAsync(_userId))
                .ReturnsAsync(new Profile { Id = _userId, Name = "Test User" });
        }

        // --- CREATE POST ---
        [Test]
        public async Task CreatePostAsync_ShouldAddPost()
        {
            Post? captured = null;
            _mockPostRepo
                .Setup(r => r.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => captured = p)
                .Returns(Task.CompletedTask);

            var postId = await _service.CreatePostAsync(_userId, "Title", "Content", null);

            Assert.IsNotNull(captured);
            Assert.That(captured.AuthorId, Is.EqualTo(_userId));
            Assert.That(captured.Title, Is.EqualTo("Title"));
            Assert.That(captured.Content, Is.EqualTo("Content"));
            Assert.That(captured.Image, Is.Null);
            Assert.That(captured.Id, Is.EqualTo(postId));
            _mockPostRepo.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        }

        [Test]
        public async Task CreatePostAsync_WithImageOnly_ShouldAddPost()
        {
            Post? captured = null;
            var image = new byte[] { 1, 2, 3 };
            _mockPostRepo
                .Setup(r => r.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => captured = p)
                .Returns(Task.CompletedTask);

            var postId = await _service.CreatePostAsync(_userId, "Title", null, image);

            Assert.IsNotNull(captured);
            Assert.That(captured.Content, Is.Null);
            Assert.That(captured.Image, Is.EqualTo(image));
            Assert.That(captured.Id, Is.EqualTo(postId));
        }

        // --- UPDATE POST ---
        [Test]
        public async Task UpdatePost_ShouldUpdatePostAndCallRepository()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Old Title", "Old Content", null);
            _mockPostRepo.Setup(r => r.GetByIdAsync(_postId)).ReturnsAsync(post);

            var newImage = new byte[] { 4, 5, 6 };
            await _service.UpdatePostAsync(_postId, "New Title", "New Content", newImage);

            Assert.That(post.Title, Is.EqualTo("New Title"));
            Assert.That(post.Content, Is.EqualTo("New Content"));
            Assert.That(post.Image, Is.EqualTo(newImage));
            _mockPostRepo.Verify(r => r.UpdateAsync(post), Times.Once);
        }

        // --- DELETE POST ---
        [Test]
        public async Task DeletePost_ShouldCallRepository()
        {
            await _service.DeletePost(_postId);
            _mockPostRepo.Verify(r => r.DeleteAsync(_postId), Times.Once);
        }

        // --- ADD COMMENT ---
        [Test]
        public async Task AddComment_ShouldCallCommentRepository()
        {
            var commentAuthor = Guid.NewGuid();
            await _commentService.AddComment(_postId, commentAuthor, "Nice!", null);

            _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Test]
        public async Task AddComment_WithImageOnly_ShouldCallCommentRepository()
        {
            var commentAuthor = Guid.NewGuid();
            var image = new byte[] { 7, 8, 9 };
            await _commentService.AddComment(_postId, commentAuthor, null, image);

            _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        }

        // --- UPDATE COMMENT ---
        [Test]
        public async Task UpdateComment_ShouldUpdateCommentContent()
        {
            var comment = Comment.CreateNewComment(_userId, "Old Comment", null);
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _commentService.UpdateCommentAsync(comment.Id, _userId, "New Comment");

            Assert.That(comment.Content, Is.EqualTo("New Comment"));
            _mockCommentRepo.Verify(r => r.UpdateAsync(comment), Times.Once);
        }

        // --- DELETE COMMENT ---
        [Test]
        public async Task DeleteComment_ShouldCallRepository()
        {
            var comment = Comment.CreateNewComment(_userId, "To Delete", null);
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _commentService.DeleteComment(_postId, comment.Id, _userId);

            _mockCommentRepo.Verify(r => r.DeleteAsync(comment.Id), Times.Once);
        }

        // --- VOTE POST ---
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
