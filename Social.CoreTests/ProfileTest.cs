using Social.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialCoreTests
{
    public class ProfileTests
    {
        [Test]
        public void UpdateProfile_ShouldChangeValues_WhenNotNull()
        {
            // Arrange
            var profile = new Profile("OldName");

            // Act
            profile.UpdateProfile("NewName", new byte[] { 1, 2, 3 }, "NewBio");

            // Assert
            Assert.That(profile.Name, Is.EqualTo("NewName"));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, profile.ProfilePic);
            Assert.That(profile.Bio, Is.EqualTo("NewBio"));
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
            Assert.IsTrue(profile.Friends.Contains(friendId));
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
