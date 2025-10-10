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
