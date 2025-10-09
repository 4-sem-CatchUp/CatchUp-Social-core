using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;

namespace SocialCoreTests.Infrastructure.Persistens
{
    [TestFixture]
    public class PostDbAdapterTests
    {
        private SocialDbContext _context;
        private PostDbAdapter _postAdapter;
        private PostService _postService;

        // For simplicity, vi laver in-memory repositories
        private VoteDbAdapter _voteAdapter;
        private CommentDbAdapter _commentAdapter;
        private SubscribeServiceStub _subscribeService;
        private FakeProfileRepository _profileAdapter;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new SocialDbContext(options);

            _postAdapter = new PostDbAdapter(_context);
            _voteAdapter = new VoteDbAdapter(_context);
            _commentAdapter = new CommentDbAdapter(_context);

            _subscribeService = new SubscribeServiceStub();
            _profileAdapter = new FakeProfileRepository();

            _postService = new PostService(
                _postAdapter,
                _commentAdapter,
                _voteAdapter,
                _profileAdapter,
                _subscribeService
            );
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreatePost_Should_Create_Post_With_Images()
        {
            var authorId = Guid.NewGuid();
            var images = new List<Image>
            {
                new Image("pic.jpg", "image/jpeg", new byte[] { 1, 2, 3 }),
            };

            var postId = await _postService.CreatePostAsync(
                authorId,
                "Test Post",
                "Content",
                images
            );

            var dbPost = await _postAdapter.GetByIdAsync(postId);

            Assert.That(dbPost, Is.Not.Null);
            Assert.That(dbPost!.Title, Is.EqualTo("Test Post"));
            Assert.That(dbPost.Content, Is.EqualTo("Content"));
            Assert.That(dbPost.Images.Count, Is.EqualTo(1));
            Assert.That(dbPost.Images[0].FileName, Is.EqualTo("pic.jpg"));
        }

        [Test]
        public async Task VotePost_Should_Add_New_Vote()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Post", "Content");
            await _postAdapter.AddAsync(post);

            var voterId = Guid.NewGuid();
            await _postService.VotePost(post.Id, true, voterId);

            var dbPost = await _postAdapter.GetByIdAsync(post.Id);

            Assert.That(dbPost.Votes.Count, Is.EqualTo(1));
            Assert.That(dbPost.Votes[0].UserId, Is.EqualTo(voterId));
            Assert.That(dbPost.Votes[0].Upvote, Is.True);
        }

        [Test]
        public async Task AddComment_Should_Add_New_Comment()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Post", "Content");
            await _postAdapter.AddAsync(post);

            var comment = Comment.CreateNewComment(Guid.NewGuid(), "Nice post!");
            await _commentAdapter.AddAsync(comment); // simulér service

            post.AddComment(comment);
            await _postAdapter.UpdateAsync(post);

            var dbPost = await _postAdapter.GetByIdAsync(post.Id);
            Assert.That(dbPost.Comments.Count, Is.EqualTo(1));
            Assert.That(dbPost.Comments[0].Content, Is.EqualTo("Nice post!"));
        }

        [Test]
        public async Task UpdatePost_Should_Update_Title_And_Content()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Old Title", "Old Content");
            await _postAdapter.AddAsync(post);

            await _postService.UpdatePostAsync(post.Id, "New Title", "New Content");

            var dbPost = await _postAdapter.GetByIdAsync(post.Id);
            Assert.That(dbPost.Title, Is.EqualTo("New Title"));
            Assert.That(dbPost.Content, Is.EqualTo("New Content"));
        }

        [Test]
        public async Task DeletePost_Should_Remove_Post()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Delete Me", "Content");
            await _postAdapter.AddAsync(post);

            await _postService.DeletePost(post.Id);

            var dbPost = await _postAdapter.GetByIdAsync(post.Id);
            Assert.That(dbPost, Is.Null);
        }
    }

    // Stub implementering af SubscribeService til test
    public class SubscribeServiceStub : ISubscribeUseCases
    {
        public Task Notify(Profile profile, string message)
        {
            // Gør ingenting, blot for at kunne køre testen
            return Task.CompletedTask;
        }

        public void Subscribe(Profile subscriber, Profile publisher) =>
            throw new NotImplementedException();

        public void Unsubscribe(Profile subscriber, Profile publisher) =>
            throw new NotImplementedException();
    }

    public class FakeProfileRepository : IProfileRepository
    {
        public Task AddFriendAsync(Guid profileId, Guid friendId) =>
            throw new NotImplementedException();

        public Task AddProfileAsync(Profile profile) => throw new NotImplementedException();

        public Task DeleteProfileAsync(Guid profileId) => throw new NotImplementedException();

        public Task<IEnumerable<Profile>> GetAllProfilesAsync() =>
            throw new NotImplementedException();

        public Task<Profile?> GetProfileByIdAsync(Guid id)
        {
            // Returner en dummy profil
            return Task.FromResult<Profile?>(
                new Profile
                {
                    Id = id,
                    Name = "TestUser",
                    Bio = "Test bio",
                }
            );
        }

        public Task UpdateProfileAsync(Profile profile) => throw new NotImplementedException();
    }
}
