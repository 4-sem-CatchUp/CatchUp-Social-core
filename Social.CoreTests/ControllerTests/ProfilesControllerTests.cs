using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Social.Controllers;
using Social.Core;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace SocialCoreTests.ControllerTests
{
    [TestFixture]
    public class ProfilesControllerTests
    {
        private Mock<IProfileUseCases> _useCasesMock;
        private Mock<IProfileRepository> _repoMock;
        private Mock<ILogger<ProfilesController>> _loggerMock;
        private ProfilesController _controller;

        [SetUp]
        public void Setup()
        {
            _useCasesMock = new Mock<IProfileUseCases>();
            _repoMock = new Mock<IProfileRepository>();
            _loggerMock = new Mock<ILogger<ProfilesController>>();

            _controller = new ProfilesController(
                _loggerMock.Object,
                _useCasesMock.Object,
                _repoMock.Object
            );
        }

        // ================================================================
        //  CreateProfile
        // ================================================================

        [Test]
        public async Task CreateProfile_ReturnsCreated_WhenValidRequest()
        {
            // Arrange
            var request = new CreateProfileRequest { UserName = "Alice" };
            var newId = Guid.NewGuid();

            _useCasesMock.Setup(u => u.CreateProfileAsync(request.UserName)).ReturnsAsync(newId);

            // Act
            var result = await _controller.CreateProfile(request);

            // Assert
            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            var created = result as CreatedAtActionResult;

            Assert.That(created!.StatusCode ?? 201, Is.EqualTo(201));
            Assert.That(created.ActionName, Is.EqualTo(nameof(_controller.GetProfile)));

            dynamic response = created.Value!;
            Assert.That(response.ProfileId, Is.EqualTo(newId));
        }

        [Test]
        public async Task CreateProfile_ReturnsBadRequest_WhenUserNameIsMissing()
        {
            // Arrange
            var request = new CreateProfileRequest { UserName = "" };

            // Act
            var result = await _controller.CreateProfile(request);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ================================================================
        //  GetProfile
        // ================================================================

        [Test]
        public async Task GetProfile_ReturnsOk_WhenProfileExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var profile = new Profile { Id = id, Name = "Alice" };

            _repoMock.Setup(r => r.GetProfileByIdAsync(id)).ReturnsAsync(profile);

            // Act
            var result = await _controller.GetProfile(id);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var ok = result as OkObjectResult;

            Assert.That(ok!.StatusCode, Is.EqualTo(200));
            Assert.That(ok.Value, Is.SameAs(profile));
        }

        [Test]
        public async Task GetProfile_ReturnsNotFound_WhenProfileMissing()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetProfileByIdAsync(id)).ReturnsAsync((Profile?)null);

            // Act
            var result = await _controller.GetProfile(id);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // ================================================================
        //  AddFriend
        // ================================================================

        [Test]
        public async Task AddFriend_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var friendId = Guid.NewGuid();

            _useCasesMock
                .Setup(u => u.AddFriendAsync(profileId, friendId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddFriend(profileId, friendId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task AddFriend_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var friendId = Guid.NewGuid();

            _useCasesMock
                .Setup(u => u.AddFriendAsync(profileId, friendId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.AddFriend(profileId, friendId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AddFriend_ReturnsServerError_WhenUnexpectedExceptionThrown()
        {
            // Arrange
            var profileId = Guid.NewGuid();
            var friendId = Guid.NewGuid();

            _useCasesMock
                .Setup(u => u.AddFriendAsync(profileId, friendId))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.AddFriend(profileId, friendId);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;

            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value, Is.EqualTo("An unexpected error occurred"));
        }
    }
}
