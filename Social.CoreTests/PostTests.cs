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
            Assert.AreEqual(_authorId, _post.AuthorId);
            Assert.AreEqual("Title", _post.Title);
            Assert.AreEqual("Content", _post.Content);
            Assert.AreEqual(0, _post.Votes.Count);
            Assert.AreEqual(0, _post.Comments.Count);
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
            Assert.AreEqual(1, _post.Votes.Count);
            Assert.AreEqual(1, _post.Karma);

            _post.AddVote(userId, false); // ændrer til downvote
            Assert.AreEqual(1, _post.Votes.Count);
            Assert.AreEqual(-1, _post.Karma);
        }

        [Test]
        public void AddVote_ShouldRemoveVote_WhenSameVoteRepeated()
        {
            var userId = Guid.NewGuid();
            _post.AddVote(userId, true);
            _post.AddVote(userId, true); // samme vote = fjernes

            Assert.AreEqual(0, _post.Votes.Count);
            Assert.AreEqual(0, _post.Karma);
        }

        [Test]
        public void AddComment_ShouldAddComment()
        {
            var commentAuthor = Guid.NewGuid();
            _post.AddComment(commentAuthor, "Nice post!");

            Assert.AreEqual(1, _post.Comments.Count);
            Assert.AreEqual(commentAuthor, _post.Comments[0].AuthorId);
            Assert.AreEqual("Nice post!", _post.Comments[0].Content);
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
            Assert.AreEqual(newTitle, _post.Title);
            Assert.AreEqual(newContent, _post.Content);
        }

        [Test]
        public void UpdateComment_ShouldChangeContent()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Title", "Content");
            post.AddComment(Guid.NewGuid(), "Old Comment");
            var comment = post.Comments.First();

            comment.UpdateComment("New Comment");

            Assert.AreEqual("New Comment", comment.Content);
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
            Assert.AreEqual(0, _post.Comments.Count);
        }
    }

}
