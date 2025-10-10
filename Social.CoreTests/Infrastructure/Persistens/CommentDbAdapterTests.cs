using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Social.Core;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;
using Social.Infrastructure.Persistens.Entities;

namespace SocialCoreTests.Infrastructure.Persistens
{
    [TestFixture]
    public class CommentDbAdapterTests
    {
        private SocialDbContext _context;
        private CommentDbAdapter _adapter;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unik DB pr. test
                .Options;

            _context = new SocialDbContext(options);
            _adapter = new CommentDbAdapter(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_Should_Add_Comment_To_Database()
        {
            // Arrange
            var comment = Comment.CreateNewComment(Guid.NewGuid(), "Test comment");

            // Act
            await _adapter.AddAsync(comment);

            // Assert
            var dbComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id);
            Assert.That(dbComment, Is.Not.Null);
            Assert.That(dbComment.Content, Is.EqualTo("Test comment"));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Comment_When_Exists()
        {
            // Arrange
            var entity = new CommentEntity
            {
                Id = Guid.NewGuid(),
                AuthorId = Guid.NewGuid(),
                Content = "Existing comment",
                Timestamp = DateTime.UtcNow,
            };
            _context.Comments.Add(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _adapter.GetByIdAsync(entity.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Content, Is.EqualTo("Existing comment"));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _adapter.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetByPostIdAsync_Should_Return_All_Comments_For_Post()
        {
            // Arrange
            var postId = Guid.NewGuid();

            _context.Comments.AddRange(
                new[]
                {
                    new CommentEntity
                    {
                        Id = Guid.NewGuid(),
                        AuthorId = Guid.NewGuid(),
                        Content = "Comment A",
                        PostId = postId,
                    },
                    new CommentEntity
                    {
                        Id = Guid.NewGuid(),
                        AuthorId = Guid.NewGuid(),
                        Content = "Comment B",
                        PostId = postId,
                    },
                    new CommentEntity
                    {
                        Id = Guid.NewGuid(),
                        AuthorId = Guid.NewGuid(),
                        Content = "Other post",
                        PostId = Guid.NewGuid(),
                    },
                }
            );

            await _context.SaveChangesAsync();

            // Act
            var result = await _adapter.GetByPostIdAsync(postId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(c => c.Content == "Comment A"));
            Assert.That(result.Any(c => c.Content == "Comment B"));
        }

        [Test]
        public async Task UpdateAsync_Should_Update_Content()
        {
            // Arrange
            var entity = new CommentEntity
            {
                Id = Guid.NewGuid(),
                AuthorId = Guid.NewGuid(),
                Content = "Old content",
            };
            _context.Comments.Add(entity);
            await _context.SaveChangesAsync();

            var updatedComment = new Comment(
                entity.AuthorId,
                "New content",
                entity.Timestamp,
                new List<Vote>()
            )
            { };
            typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(updatedComment, entity.Id);

            // Act
            await _adapter.UpdateAsync(updatedComment);

            // Assert
            var dbComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == entity.Id);
            Assert.That(dbComment!.Content, Is.EqualTo("New content"));
        }

        [Test]
        public async Task DeleteAsync_Should_Remove_Comment()
        {
            // Arrange
            var entity = new CommentEntity
            {
                Id = Guid.NewGuid(),
                AuthorId = Guid.NewGuid(),
                Content = "To delete",
            };
            _context.Comments.Add(entity);
            await _context.SaveChangesAsync();

            // Act
            await _adapter.DeleteAsync(entity.Id);

            // Assert
            var exists = await _context.Comments.AnyAsync(c => c.Id == entity.Id);
            Assert.That(exists, Is.False);
        }
    }
}
