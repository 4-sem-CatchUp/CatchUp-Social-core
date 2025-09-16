using Social.Core;

namespace SocialCoreTests
{
    public class PostTests
    {
        private Post _post;
        private Guid _authorId;

        [SetUp]
        public void Setup()
        {
            _authorId = Guid.NewGuid();
            _post = Post.CreateNewPost(_authorId, "Title", "Content");
        }

        // --- CREATE ---
        [Test]
        public void CreateNewPost_ShouldInitializeProperties()
        {
            Assert.That(_post.AuthorId, Is.EqualTo(_authorId));
            Assert.That(_post.Title, Is.EqualTo("Title"));
            Assert.That(_post.Content, Is.EqualTo("Content"));
            Assert.That(_post.Votes.Count, Is.EqualTo(0));
            Assert.That(_post.Comments.Count, Is.EqualTo(0));
        }

        // --- READ / GET ---
        [Test]
        public void GetUserVote_ShouldReturnCorrectVote()
        {
            var userId = Guid.NewGuid();
            _post.AddVote(userId, true);

            var vote = _post.GetUserVote(userId);
            Assert.IsNotNull(vote);
            Assert.IsTrue(vote.Value);
        }

        [Test]
        public void GetUserVote_ShouldReturnNull_WhenNoVoteExists()
        {
            var userId = Guid.NewGuid();
            var vote = _post.GetUserVote(userId);

            Assert.IsNull(vote);
        }

        // --- UPDATE ---
        [Test]
        public void AddVote_ShouldAddVoteOrUpdateKarma()
        {
            var userId = Guid.NewGuid();
            _post.AddVote(userId, true);
            Assert.That(_post.Votes.Count, Is.EqualTo(1));
            Assert.That(_post.Karma, Is.EqualTo(1));

            _post.AddVote(userId, false); // ændrer til downvote
            Assert.That(_post.Votes.Count, Is.EqualTo(1));
            Assert.That(_post.Karma, Is.EqualTo(-1));
        }

        [Test]
        public void AddVote_ShouldRemoveVote_WhenSameVoteRepeated()
        {
            var userId = Guid.NewGuid();
            _post.AddVote(userId, true);
            _post.AddVote(userId, true); // samme vote = fjernes

            Assert.That(_post.Votes.Count, Is.EqualTo(0));
            Assert.That(_post.Karma, Is.EqualTo(0));
        }

        [Test]
        public void AddComment_ShouldAddComment()
        {
            var commentAuthor = Guid.NewGuid();
            _post.AddComment(commentAuthor, "Nice post!");

            Assert.That(_post.Comments.Count, Is.EqualTo(1));
            Assert.That(_post.Comments[0].AuthorId, Is.EqualTo(commentAuthor));
            Assert.That(_post.Comments[0].Content, Is.EqualTo("Nice post!"));
        }

        [Test]
        public void UpdatePost_ShouldChangeTitleAndContent()
        {
            // Arrange
            var newTitle = "Updated Title";
            var newContent = "Updated Content";

            // Act
            _post.Title = newTitle;
            _post.Content = newContent;

            // Assert
            Assert.That(_post.Title, Is.EqualTo(newTitle));
            Assert.That(_post.Content, Is.EqualTo(newContent));
        }

        [Test]
        public void UpdateComment_ShouldChangeContent()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Title", "Content");
            post.AddComment(Guid.NewGuid(), "Old Comment");
            var comment = post.Comments.First();

            comment.UpdateComment("New Comment");

            Assert.That(comment.Content, Is.EqualTo("New Comment"));
        }

        // --- DELETE ---
        [Test]
        public void RemoveComment_ShouldDeleteComment()
        {
            // Arrange
            var commentAuthor = Guid.NewGuid();
            _post.AddComment(commentAuthor, "A comment");
            var commentId = _post.Comments[0].Id;

            // Act
            _post.RemoveComment(commentId); // vi laver denne metode i Post
            // Assert
            Assert.That(_post.Comments.Count, Is.EqualTo(0));
        }
    }

}
