using Social.Core;
using Social.Core.Ports.Outgoing;

namespace Social.Social.Infrastructure.Persistens
{
    public class ProfileDbAdapter : IProfileRepository
    {
        public Task AddFriendAsync(Guid profileId, Guid friendId)
        {
            throw new NotImplementedException();
        }

        public Task AddProfileAsync(Profile profile)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProfileAsync(Guid profileId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Profile>> GetAllProfilesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Profile?> GetProfileByIdAsync(Guid profileId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProfileAsync(Profile profile)
        {
            throw new NotImplementedException();
        }
    }
}
