namespace Social.Core
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Bio { get; set; } = string.Empty;
        public DateOnly DateOfSub { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public List<Guid> Friends { get; private set; } = new List<Guid>();
        public Image ProfilePic { get; set; }

        public Profile()
        {
            Name = "New User";
        }

        public Profile(string name)
        {
            Name = name;
        }

        public static Profile CreateNewProfile(string name)
        {
            return new Profile(name);
        }

        public void AddImage(string fileName, string contentType, byte[] data)
        {
            ProfilePic = new Image(fileName, contentType, data);
        }

        public void UpdateProfile(string? name, string? bio, Image? pic)
        {
            if (name != null)
                Name = name;
            if (bio != null)
                Bio = bio;
            if (pic != null)
                ProfilePic = pic;
        }

        public void AddFriend(Guid friendId)
        {
            if (!Friends.Contains(friendId))
                Friends.Add(friendId);
        }
    }
}
