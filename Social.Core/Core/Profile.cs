namespace Social.Core
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public byte[] ProfilePic { get; set; } = Array.Empty<byte>();
        public string Bio { get; set; } = string.Empty;
        public DateOnly DateOfSub { get; private set; }
            = DateOnly.FromDateTime(DateTime.UtcNow);
        public List<Guid> Friends { get; private set; } = new List<Guid>();
        /// <summary>
        /// Initializes a new Profile with default values.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="Name"/> to "New User". Other properties are initialized by their field initializers (new <see cref="Id"/>; empty <see cref="ProfilePic"/> and <see cref="Bio"/>; <see cref="DateOfSub"/> set to current UTC date; empty <see cref="Friends"/> list).
        /// </remarks>
        public Profile()
        {
            Name = "New User";
        }
        /// <summary>
        /// Initializes a new instance of <see cref="Profile"/> with the specified display name.
        /// </summary>
        /// <param name="name">The profile's display name.</param>
        public Profile(string name)
        {
            Name = name;
        }
        public static Profile CreateNewProfile(string name)
        {
            return new Profile(name);
        }
        public void UpdateProfile(string? name, byte[]? profilePic, string? bio)
        {
            if (name != null)
                Name = name;
            if (profilePic != null)
                ProfilePic = profilePic;
            if (bio != null)
                Bio = bio;
        }
        public void AddFriend(Guid friendId)
        {
            if (!Friends.Contains(friendId))
                Friends.Add(friendId);
        }
    }
}
