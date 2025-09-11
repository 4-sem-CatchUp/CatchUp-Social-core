using Social.Core;

namespace Social.Core.Ports.Outgoing
{
    public interface IProfileRepository
    {
        Task AddProfileAsync(Profile profile);
        Task<Profile?> GetProfileByIdAsync(Guid profileId);
        Task UpdateProfileAsync(Profile profile);
        Task DeleteProfileAsync(Guid profileId);
        Task AddFriendAsync(Guid profileId, Guid friendId);
        Task<IEnumerable<Profile>> GetAllProfilesAsync();
    }
}
