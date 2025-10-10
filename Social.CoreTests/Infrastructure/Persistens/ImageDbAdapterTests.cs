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
    public class ImageDbAdapterTests
    {
        private SocialDbContext _context;
        private ImageDbAdapter _adapter;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new SocialDbContext(options);
            _adapter = new ImageDbAdapter(_context);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task AddAsync_Should_Add_Image()
        {
            var image = new Image("pic.jpg", "image/jpeg", new byte[] { 1, 2, 3 });

            await _adapter.AddAsync(image);

            var dbImage = await _context.Images.FirstOrDefaultAsync(i => i.Id == image.Id);
            Assert.That(dbImage, Is.Not.Null);
            Assert.That(dbImage.FileName, Is.EqualTo("pic.jpg"));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Image_When_Exists()
        {
            var entity = new ImageEntity
            {
                Id = Guid.NewGuid(),
                FileName = "pic.jpg",
                ContentType = "image/jpeg",
                Data = new byte[] { 1, 2, 3 },
            };
            _context.Images.Add(entity);
            await _context.SaveChangesAsync();

            var result = await _adapter.GetByIdAsync(entity.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FileName, Is.EqualTo("pic.jpg"));
        }

        [Test]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            var result = await _adapter.GetByIdAsync(Guid.NewGuid());
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_Should_Remove_Image()
        {
            var entity = new ImageEntity
            {
                Id = Guid.NewGuid(),
                FileName = "delete.jpg",
                ContentType = "image/jpeg",
                Data = new byte[] { 1 },
            };
            _context.Images.Add(entity);
            await _context.SaveChangesAsync();

            await _adapter.DeleteAsync(entity.Id);

            var exists = await _context.Images.AnyAsync(i => i.Id == entity.Id);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task GetByCommentIdAsync_Should_Return_All_Images_For_Comment()
        {
            var commentId = Guid.NewGuid();
            _context.Images.AddRange(
                new[]
                {
                    new ImageEntity
                    {
                        Id = Guid.NewGuid(),
                        FileName = "a.jpg",
                        CommentId = commentId,
                    },
                    new ImageEntity
                    {
                        Id = Guid.NewGuid(),
                        FileName = "b.jpg",
                        CommentId = commentId,
                    },
                    new ImageEntity
                    {
                        Id = Guid.NewGuid(),
                        FileName = "c.jpg",
                        CommentId = Guid.NewGuid(),
                    },
                }
            );
            await _context.SaveChangesAsync();

            var result = await _adapter.GetByCommentIdAsync(commentId);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Any(i => i.FileName == "a.jpg"));
            Assert.That(result.Any(i => i.FileName == "b.jpg"));
        }

        [Test]
        public async Task GetByProfileIdAsync_Should_Return_Only_Profile_Image()
        {
            var profileId = Guid.NewGuid();
            var entity = new ImageEntity
            {
                Id = Guid.NewGuid(),
                FileName = "profile.jpg",
                ProfileId = profileId,
            };
            _context.Images.Add(entity);
            await _context.SaveChangesAsync();

            var result = await _adapter.GetByProfileIdAsync(profileId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FileName, Is.EqualTo("profile.jpg"));
        }

        [Test]
        public async Task GetByProfileIdAsync_Should_Return_Null_When_No_Image()
        {
            var result = await _adapter.GetByProfileIdAsync(Guid.NewGuid());
            Assert.That(result, Is.Null);
        }
    }
}
