using Social.Core;
using Social.Core.Ports.Incomming;
using Social.Core.Ports.Outgoing;

namespace Social.Core.Application
{
    public class ProfileService : IProfileUseCases
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<Guid> CreateProfileAsync(string userName)
        {
            var profile = Profile.CreateNewProfile(userName);
            await _profileRepository.AddProfileAsync(profile);
            return profile.Id;
        }

        /// <summary>
        /// Updates the specified profile's name, bio, and profile picture, then persists the change to the repository.
        /// </summary>
        /// <param name="profileId">The identifier of the profile to update.</param>
        /// <param name="name">The new display name for the profile, or null to leave unchanged.</param>
        /// <param name="profilePic">The new profile image, or null to leave unchanged.</param>
        /// <param name="bio">The new biography text, or null to leave unchanged.</param>
        /// <exception cref="InvalidOperationException">Thrown if no profile exists for the given <paramref name="profileId"/>.</exception>
        public async Task UpdateProfileAsync(
            Guid profileId,
            string? name,
            Image? profilePic,
            string? bio
        )
        {
            var profile =
                await _profileRepository.GetProfileByIdAsync(profileId)
                ?? throw new InvalidOperationException("Profile not found");
            profile.UpdateProfile(name, bio, profilePic);
            await _profileRepository.UpdateProfileAsync(profile);
        }

        public async Task AddFriendAsync(Guid profileId, Guid friendId)
        {
            var profile =
                await _profileRepository.GetProfileByIdAsync(profileId)
                ?? throw new InvalidOperationException("Profile not found");
            if (await _profileRepository.GetProfileByIdAsync(friendId) != null)
            {
                profile.AddFriend(friendId);
            }
            else
                throw new InvalidOperationException("Friend not found");
            await _profileRepository.UpdateProfileAsync(profile);
        }
    }
}