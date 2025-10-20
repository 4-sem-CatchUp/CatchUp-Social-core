using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Social.Core;
using Social.Core.Application;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace SocialCoreTests.CoreTests.ApplicationTests
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
            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            _mockRepo.Verify(
                r => r.AddProfileAsync(It.Is<Profile>(p => p.Name == userName)),
                Times.Once
            );
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
            Assert.That(profile.Name, Is.EqualTo("NewName"));
            Assert.That(profile.Bio, Is.EqualTo("NewBio"));
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
            _mockRepo
                .Setup(r => r.GetProfileByIdAsync(friendId))
                .ReturnsAsync(new Profile("Friend"));

            // Act
            await _service.AddFriendAsync(profileId, friendId);

            // Assert
            Assert.That(profile.Friends.Contains(friendId), Is.True);
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
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _service.AddFriendAsync(profileId, friendId);
            });
        }
    }
}
