using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Social.Core;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileUseCases _profileUseCases;
        private readonly ILogger _logger;
        private readonly IProfileRepository _profileRepository;

        public ProfilesController(
            ILogger logger,
            IProfileUseCases profileUseCases,
            IProfileRepository profileRepository
        )
        {
            _profileUseCases = profileUseCases;
            _logger = logger;
            _profileRepository = profileRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
                return BadRequest("Username is required");

            _logger.LogInformation("Creating profile for user: {UserName}", request.UserName);

            var profileId = await _profileUseCases.CreateProfileAsync(request.UserName);

            var response = new CreateProfileResponse
            {
                Message = "Profile created successfully",
                ProfileId = profileId,
            };

            _logger.LogInformation("Profile created with ID: {ProfileId}", profileId);

            return CreatedAtAction(nameof(GetProfile), new { id = profileId }, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            _logger.LogInformation("Searching for profile {ProfileId}", id);
            var profile = await _profileRepository.GetProfileByIdAsync(id) ?? null;

            if (profile == null)
                return NotFound();

            _logger.LogInformation($"Profile {id}");

            return Ok(profile);
        }

        [HttpPost("{id:guid}/friends/{friendId:guid}")]
        public async Task<IActionResult> AddFriend(Guid Id, Guid friendId)
        {
            _logger.LogInformation("Adding friend {FriendId} to profile {ProfileId}", friendId, Id);

            try
            {
                await _profileUseCases.AddFriendAsync(Id, friendId);
                _logger.LogInformation(
                    "Friend {FriendId} added successfully to profile {ProfileId}",
                    friendId,
                    Id
                );
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error adding friend {FriendId} to profile {ProfileId}",
                    friendId,
                    Id
                );
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }

    public class CreateProfileRequest
    {
        public string UserName { get; set; } = null;
    }

    public class CreateProfileResponse
    {
        public string Message { get; set; } = null!;
        public Guid ProfileId { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; } = null;
        public Image? ProfilePic { get; set; } = null;
        public string? bio { get; set; } = null;
    }
}
