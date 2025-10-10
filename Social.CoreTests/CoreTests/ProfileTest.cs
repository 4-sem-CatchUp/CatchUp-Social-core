using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Core;

namespace SocialCoreTests.CoreTests
{
    public class ProfileTests
    {
        [Test]
        public void UpdateProfile_ShouldChangeValues_WhenNotNull()
        {
            // Arrange
            var profile = new Profile("OldName");
            var newImage = new Image("pic.png", "image/png", new byte[] { 1, 2, 3 });

            // Act
            profile.UpdateProfile("NewName", "NewBio", newImage);

            // Assert
            Assert.That(profile.Name, Is.EqualTo("NewName"));
            Assert.That(profile.Bio, Is.EqualTo("NewBio"));
            Assert.That(profile.ProfilePic, Is.Not.Null);
            Assert.That(profile.ProfilePic.FileName, Is.EqualTo("pic.png"));
            Assert.That(profile.ProfilePic.ContentType, Is.EqualTo("image/png"));
            Assert.That(profile.ProfilePic.Data, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public void AddImage_ShouldSetProfilePic()
        {
            // Arrange
            var profile = new Profile("User");
            var data = new byte[] { 9, 8, 7 };

            // Act
            profile.AddImage("avatar.jpg", "image/jpeg", data);

            // Assert
            Assert.That(profile.ProfilePic, Is.Not.Null);
            Assert.That(profile.ProfilePic.FileName, Is.EqualTo("avatar.jpg"));
            Assert.That(profile.ProfilePic.ContentType, Is.EqualTo("image/jpeg"));
            Assert.That(profile.ProfilePic.Data, Is.EqualTo(data));
        }

        [Test]
        public void AddFriend_ShouldAdd_WhenNotAlreadyFriend()
        {
            // Arrange
            var profile = new Profile("User");
            var friendId = Guid.NewGuid();

            // Act
            profile.AddFriend(friendId);

            // Assert
            Assert.That(profile.Friends.Contains(friendId), Is.True);
        }

        [Test]
        public void AddFriend_ShouldNotDuplicate_WhenAlreadyFriend()
        {
            // Arrange
            var profile = new Profile("User");
            var friendId = Guid.NewGuid();
            profile.AddFriend(friendId);

            // Act
            profile.AddFriend(friendId);

            // Assert
            Assert.That(profile.Friends.Count, Is.EqualTo(1));
        }
    }
}
