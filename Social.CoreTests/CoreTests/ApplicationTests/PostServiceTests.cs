using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace SocialCoreTests.CoreTests.ApplicationTests
{
    [TestFixture]
    public class PostServiceTests
    {
        private Mock<IPostRepository> _mockPostRepo;
        private Mock<ICommentRepository> _mockCommentRepo;
        private Mock<IVoteRepository> _mockVoteRepo;
        private Mock<ISubscribeUseCases> _mockSubscriptionService;
        private Mock<IProfileRepository> _mockProfileRepo;

        private IPostUseCases _service;
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

            _postId = Guid.NewGuid();
            _userId = Guid.NewGuid();
            _post = Post.CreateNewPost(_userId, "Title", "Content");

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

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.AuthorId, Is.EqualTo(_userId));
            Assert.That(captured.Title, Is.EqualTo("Title"));
            Assert.That(captured.Content, Is.EqualTo("Content"));
            Assert.That(captured.Images.Count, Is.EqualTo(0));
            Assert.That(captured.Id, Is.EqualTo(postId));
            _mockPostRepo.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        }

        [Test]
        public async Task CreatePostAsync_WithImages_ShouldAddPost()
        {
            Post? captured = null;
            var images = new List<Image>
            {
                new Image("file.jpg", "image/jpeg", new byte[] { 1, 2 }),
            };

            _mockPostRepo
                .Setup(r => r.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => captured = p)
                .Returns(Task.CompletedTask);

            var postId = await _service.CreatePostAsync(_userId, "Title", "Content", images);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.Images.Count, Is.EqualTo(1));
            Assert.That(captured.Id, Is.EqualTo(postId));
        }

        // --- UPDATE POST ---
        [Test]
        public async Task UpdatePost_ShouldUpdatePostAndCallRepository()
        {
            var post = Post.CreateNewPost(_userId, "Old Title", "Old Content");
            _mockPostRepo.Setup(r => r.GetByIdAsync(_postId)).ReturnsAsync(post);

            await _service.UpdatePostAsync(_postId, "New Title", "New Content");

            Assert.That(post.Title, Is.EqualTo("New Title"));
            Assert.That(post.Content, Is.EqualTo("New Content"));
            _mockPostRepo.Verify(r => r.UpdateAsync(post), Times.Once);
        }

        // --- DELETE POST ---
        [Test]
        public async Task DeletePost_ShouldCallRepository()
        {
            await _service.DeletePost(_postId);
            _mockPostRepo.Verify(r => r.DeleteAsync(_postId), Times.Once);
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

            Assert.That(result.Value, Is.True);
        }
    }
}
