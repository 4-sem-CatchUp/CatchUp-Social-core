using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Outgoing;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Social.Core.Ports.Incomming;

namespace SocialCoreTests
{
    public class ProfileServiceTests
    {
        private Mock<IProfileRepository> _mockRepo;
        private IProfileUseCases _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IProfileRepository>();
            _service = new ProfileService(_mockRepo.Object);
        }

        [Test]
        public async Task CreateProfileAsync_ShouldReturnProfileId_AndSaveProfile()
        {
            // Arrange
            string userName = "TestUser";

            // Act
            var result = await _service.CreateProfileAsync(userName);

            // Assert
            Assert.AreNotEqual(Guid.Empty, result);
            _mockRepo.Verify(r => r.AddProfileAsync(It.Is<Profile>(p => p.Name == userName)), Times.Once);
        }

        [Test]
        public async Task UpdateProfileAsync_ShouldUpdateProfile_WhenProfileExists()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var profile = new Profile("OldName");
            _mockRepo.Setup(r => r.GetProfileByIdAsync(profileId)).ReturnsAsync(profile);

            // Act
            await _service.UpdateProfileAsync(profileId, "NewName", null, "NewBio");

            // Assert
            Assert.AreEqual("NewName", profile.Name);
            Assert.AreEqual("NewBio", profile.Bio);
            _mockRepo.Verify(r => r.UpdateProfileAsync(profile), Times.Once);
        }

        [Test]
        public void UpdateProfileAsync_ShouldThrow_WhenProfileNotFound()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetProfileByIdAsync(profileId)).ReturnsAsync((Profile)null!);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _service.UpdateProfileAsync(profileId, "NewName", null, "NewBio");
            });
        }

        [Test]
        public async Task AddFriendAsync_ShouldAddFriend_WhenBothProfilesExist()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var friendId = Guid.NewGuid();
            var profile = new Profile("User");

            _mockRepo.Setup(r => r.GetProfileByIdAsync(profileId)).ReturnsAsync(profile);
            _mockRepo.Setup(r => r.GetProfileByIdAsync(friendId)).ReturnsAsync(new Profile("Friend"));

            // Act
            await _service.AddFriendAsync(profileId, friendId);

            // Assert
            Assert.IsTrue(profile.Friends.Contains(friendId));
            _mockRepo.Verify(r => r.UpdateProfileAsync(profile), Times.Once);
        }

        [Test]
        public void AddFriendAsync_ShouldThrow_WhenFriendNotFound()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var friendId = Guid.NewGuid();
            var profile = new Profile("User");

            _mockRepo.Setup(r => r.GetProfileByIdAsync(profileId)).ReturnsAsync(profile);
            _mockRepo.Setup(r => r.GetProfileByIdAsync(friendId)).ReturnsAsync((Profile)null!);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _service.AddFriendAsync(profileId, friendId);
            });
        }
    }
}
