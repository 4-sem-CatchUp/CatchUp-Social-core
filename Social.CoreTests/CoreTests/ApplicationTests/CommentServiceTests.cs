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
    public class CommentServiceTests
    {
        private Mock<IPostRepository> _mockPostRepo;
        private Mock<ICommentRepository> _mockCommentRepo;
        private Mock<IVoteRepository> _mockVoteRepo;
        private Mock<IProfileRepository> _mockProfileRepo;
        private Mock<ISubscribeUseCases> _mockSubscriptionService;

        private ICommentUseCases _service;
        private Guid _postId;
        private Guid _userId;
        private Post _post;

        [SetUp]
        public void Setup()
        {
            _mockPostRepo = new Mock<IPostRepository>();
            _mockCommentRepo = new Mock<ICommentRepository>();
            _mockVoteRepo = new Mock<IVoteRepository>();
            _mockProfileRepo = new Mock<IProfileRepository>();
            _mockSubscriptionService = new Mock<ISubscribeUseCases>();

            _service = new CommentServices(
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

        // --- ADD COMMENT ---
        [Test]
        public async Task AddComment_ShouldCallCommentRepository()
        {
            await _service.AddComment(_postId, _userId, "Nice comment!", null);

            _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Test]
        public async Task AddComment_WithImages_ShouldCallCommentRepository()
        {
            var images = new List<Image>
            {
                new Image("file.jpg", "image/jpeg", new byte[] { 1, 2 }),
            };

            await _service.AddComment(_postId, _userId, "Image comment", images);

            _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        }

        // --- UPDATE COMMENT ---
        [Test]
        public async Task UpdateComment_ShouldUpdateCommentContent()
        {
            var comment = Comment.CreateNewComment(_userId, "Old Comment");
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _service.UpdateCommentAsync(comment.Id, _userId, "New Comment");

            Assert.That(comment.Content, Is.EqualTo("New Comment"));
            _mockCommentRepo.Verify(r => r.UpdateAsync(comment), Times.Once);
        }

        // --- DELETE COMMENT ---
        [Test]
        public async Task DeleteComment_ShouldCallRepository()
        {
            var comment = Comment.CreateNewComment(_userId, "To Delete");
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _service.DeleteComment(_postId, comment.Id, _userId);

            _mockCommentRepo.Verify(r => r.DeleteAsync(comment.Id), Times.Once);
        }

        // --- VOTE COMMENT ---
        [Test]
        public async Task VoteComment_ShouldCallVoteRepository()
        {
            var comment = Comment.CreateNewComment(_userId, "Vote me");
            _mockCommentRepo.Setup(r => r.GetByIdAsync(comment.Id)).ReturnsAsync(comment);

            await _service.VoteComment(comment.Id, true, _userId);

            _mockVoteRepo.Verify(r => r.AddAsync(It.IsAny<Vote>()), Times.Once);
        }

        [Test]
        public async Task GetUserCommentVote_ShouldReturnVote()
        {
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                TargetId = Guid.NewGuid(),
                UserId = _userId,
                Upvote = true,
                VoteTargetType = VoteTargetType.Comment,
            };

            _mockVoteRepo
                .Setup(r => r.GetUserVoteAsync(It.IsAny<Guid>(), VoteTargetType.Comment, _userId))
                .ReturnsAsync(vote);

            var result = await _service.GetUserCommentVote(_postId, vote.Id, _userId);

            Assert.That(result.Value, Is.True);
        }
    }
}
