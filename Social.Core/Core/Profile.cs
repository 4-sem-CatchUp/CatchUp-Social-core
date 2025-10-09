namespace Social.Core
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Bio { get; set; } = string.Empty;
        public DateOnly DateOfSub { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public List<Guid> Friends { get; private set; } = new List<Guid>();
        public Image ProfilePic { get; set; }

        /// <summary>
        /// Initializes a new Profile with default values.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="Name"/> to "New User" and leaves other properties at their declared defaults (new Id, empty Bio, current UTC DateOfSub, empty Friends list, and null ProfilePic).
        /// </remarks>
        public Profile()
        {
            Name = "New User";
        }

        /// <summary>
        /// Initializes a new <see cref="Profile"/> with the specified display name.
        /// </summary>
        /// <param name="name">The profile's display name.</param>
        /// <remarks>
        /// Other properties are initialized to their defaults: <see cref="Id"/> is a new GUID, <see cref="Bio"/> is an empty string, <see cref="DateOfSub"/> is set to the current UTC date, <see cref="Friends"/> is an empty list, and <see cref="ProfilePic"/> is null.
        /// </remarks>
        public Profile(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a Profile with the specified identifier, name, bio, subscription date, friend list, and profile image.
        /// </summary>
        /// <param name="id">The profile's unique identifier.</param>
        /// <param name="name">The profile's display name.</param>
        /// <param name="bio">The profile's biography text.</param>
        /// <param name="dateOfSub">The date the profile subscribed or was created.</param>
        /// <param name="friends">A list of friend IDs; may be null.</param>
        /// <param name="profilePic">The profile image; may be null.</param>
        public Profile(
            Guid id,
            string name,
            string bio,
            DateOnly dateOfSub,
            List<Guid>? friends,
            Image? profilePic
        )
        {
            Id = id;
            Name = name;
            Bio = bio;
            DateOfSub = dateOfSub;
            Friends = friends;
            ProfilePic = profilePic;
        }

        /// <summary>
        /// Create a Profile initialized with the specified name.
        /// </summary>
        /// <param name="name">The display name for the new profile.</param>
        /// <returns>A Profile instance whose Name is set to the provided value.</returns>
        public static Profile CreateNewProfile(string name)
        {
            return new Profile(name);
        }

        /// <summary>
        /// Sets the profile picture by creating a new Image from the provided file name, MIME type, and binary data.
        /// </summary>
        /// <param name="fileName">The original file name of the image.</param>
        /// <param name="contentType">The image MIME type (e.g., "image/png").</param>
        /// <param name="data">The raw binary data of the image.</param>
        public void AddImage(string fileName, string contentType, byte[] data)
        {
            ProfilePic = new Image(fileName, contentType, data);
        }

        /// <summary>
        /// Set the profile's picture to the provided Image (used when reconstructing from the database).
        /// </summary>
        /// <param name="image">The Image to assign to the ProfilePic property.</param>
        public void AddImage(Image image)
        {
            ProfilePic = image;
        }

        /// <summary>
        /// Updates the profile's Name, Bio, and ProfilePic with any non-null values provided.
        /// </summary>
        /// <param name="name">The new name to assign when not null.</param>
        /// <param name="bio">The new biography text to assign when not null.</param>
        /// <param name="pic">The new profile image to assign when not null.</param>
        public void UpdateProfile(string? name, string? bio, Image? pic)
        {
            if (name != null)
                Name = name;
            if (bio != null)
                Bio = bio;
            if (pic != null)
                ProfilePic = pic;
        }

        /// <summary>
        /// Adds the specified friend's id to the profile's friends list if it is not already present.
        /// </summary>
        /// <param name="friendId">The identifier of the friend to add.</param>
        public void AddFriend(Guid friendId)
        {
            if (!Friends.Contains(friendId))
                Friends.Add(friendId);
        }
    }
}