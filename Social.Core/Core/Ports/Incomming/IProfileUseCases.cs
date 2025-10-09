namespace Social.Core.Ports.Incomming
{
    public interface IProfileUseCases
    {
        /// <summary>
/// Creates a new profile for the specified user name.
/// </summary>
/// <param name="userName">The user name to associate with the new profile.</param>
/// <returns>The Guid of the created profile.</returns>
Task<Guid> CreateProfileAsync(string userName);
        /// <summary>
/// Updates fields of an existing profile.
/// </summary>
/// <param name="profileId">The identifier of the profile to update.</param>
/// <param name="name">The new display name, or <c>null</c> to leave the name unchanged.</param>
/// <param name="profilePic">The new profile image, or <c>null</c> to leave the picture unchanged.</param>
/// <param name="bio">The new biography text, or <c>null</c> to leave the bio unchanged.</param>
Task UpdateProfileAsync(Guid profileId, string? name, Image? profilePic, string? bio);
        /// <summary>
/// Creates a friendship relation between the profile identified by <paramref name="profileId"/> and the profile identified by <paramref name="friendId"/>.
/// </summary>
/// <param name="profileId">The identifier of the profile initiating the friend addition.</param>
/// <param name="friendId">The identifier of the profile to add as a friend.</param>
Task AddFriendAsync(Guid profileId, Guid friendId);
    }
}