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
            if (profileId == friendId)
                throw new InvalidOperationException("You cannot add yourself as a friend.");

            var profile =
                await _profileRepository.GetProfileByIdAsync(profileId)
                ?? throw new KeyNotFoundException("Profile not found");

            var friend =
                await _profileRepository.GetProfileByIdAsync(friendId)
                ?? throw new KeyNotFoundException("Friend not found");

            profile.AddFriend(friendId);

            await _profileRepository.UpdateProfileAsync(profile);
        }
    }
}
