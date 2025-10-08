using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework.Legacy;
using Social.Core;
using Social.Infrastructure.Persistens;
using Social.Infrastructure.Persistens.dbContexts;

namespace SocialCoreTests.Infrastructure.Persistens
{
    [TestFixture]
    public class ProfileDbAdapterTests
    {
        private SocialDbContext _dbContext;
        private ProfileDbAdapter _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SocialDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new SocialDbContext(options);
            _dbContext.Database.EnsureCreated();

            _repository = new ProfileDbAdapter(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task AddProfileAsync_Should_Add_Profile()
        {
            var profile = new Profile
            {
                Name = "Test User",
                Bio = "Bio",
                ProfilePic = new Image("pic.jpg", "image/jpeg", new byte[] { 1, 2, 3 }),
            };

            await _repository.AddProfileAsync(profile);

            var fromDb = await _repository.GetProfileByIdAsync(profile.Id);

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(profile.Name, Is.EqualTo(fromDb!.Name));
            Assert.That(profile.Bio, Is.EqualTo(fromDb.Bio));
            Assert.That(fromDb.ProfilePic, Is.Not.Null);
            Assert.That(profile.ProfilePic.FileName, Is.EqualTo(fromDb.ProfilePic!.FileName));
        }

        [Test]
        public async Task UpdateProfileAsync_Should_Update_Existing_Profile()
        {
            var profile = new Profile { Name = "Original", Bio = "Original Bio" };
            await _repository.AddProfileAsync(profile);

            profile.Name = "Updated";
            profile.Bio = "Updated Bio";
            await _repository.UpdateProfileAsync(profile);

            var fromDb = await _repository.GetProfileByIdAsync(profile.Id);

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(fromDb!.Name, Is.EqualTo("Updated"));
            Assert.That(fromDb.Bio, Is.EqualTo("Updated Bio"));
        }

        [Test]
        public async Task DeleteProfileAsync_Should_Remove_Profile()
        {
            var profile = new Profile { Name = "To Delete" };
            await _repository.AddProfileAsync(profile);

            await _repository.DeleteProfileAsync(profile.Id);

            var fromDb = await _repository.GetProfileByIdAsync(profile.Id);
            Assert.That(fromDb, Is.Null);
        }

        [Test]
        public async Task AddFriendAsync_Should_Add_Friend()
        {
            var profile = new Profile { Name = "User 1" };
            var friend = new Profile { Name = "User 2" };

            await _repository.AddProfileAsync(profile);
            await _repository.AddProfileAsync(friend);

            await _repository.AddFriendAsync(profile.Id, friend.Id);

            var fromDb = await _repository.GetProfileByIdAsync(profile.Id);

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(fromDb!.Friends, Has.Member(friend.Id));
        }

        [Test]
        public async Task GetAllProfilesAsync_Should_Return_All_Profiles()
        {
            var profile1 = new Profile { Name = "User 1" };
            var profile2 = new Profile { Name = "User 2" };

            await _repository.AddProfileAsync(profile1);
            await _repository.AddProfileAsync(profile2);

            var allProfiles = (await _repository.GetAllProfilesAsync()).ToList();

            Assert.That(allProfiles.Count, Is.EqualTo(2));
            Assert.That(allProfiles.Any(p => p.Name == "User 1"), Is.True);
            Assert.That(allProfiles.Any(p => p.Name == "User 2"), Is.True);
        }

        [Test]
        public async Task UpdateProfileAsync_Should_Update_ProfilePic()
        {
            var profile = new Profile { Name = "User" };
            await _repository.AddProfileAsync(profile);

            profile.ProfilePic = new Image("newpic.png", "image/png", new byte[] { 1, 2, 3 });
            await _repository.UpdateProfileAsync(profile);

            var fromDb = await _repository.GetProfileByIdAsync(profile.Id);

            Assert.That(fromDb?.ProfilePic, Is.Not.Null);
            Assert.That(fromDb!.ProfilePic!.FileName, Is.EqualTo("newpic.png"));
        }
    }
}
