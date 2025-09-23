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
            _post = Post.CreateNewPost(_authorId, "Title", "Content", null);
        }

        // --- CREATE ---
        [Test]
        public void CreateNewPost_ShouldInitializeProperties()
        {
            Assert.That(_post.AuthorId, Is.EqualTo(_authorId));
            Assert.That(_post.Title, Is.EqualTo("Title"));
            Assert.That(_post.Content, Is.EqualTo("Content"));
            Assert.That(_post.Image, Is.Null);
            Assert.That(_post.Votes.Count, Is.EqualTo(0));
            Assert.That(_post.Comments.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateNewPost_WithImageOnly_ShouldHaveNullContent()
        {
            var image = new byte[] { 1, 2, 3 };
            var postWithImage = Post.CreateNewPost(_authorId, "Title", null, image);

            Assert.That(postWithImage.Content, Is.Null);
            Assert.That(postWithImage.Image, Is.EqualTo(image));
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
            _post.AddComment(commentAuthor, "Nice post!", null);

            Assert.That(_post.Comments.Count, Is.EqualTo(1));
            Assert.That(_post.Comments[0].AuthorId, Is.EqualTo(commentAuthor));
            Assert.That(_post.Comments[0].Content, Is.EqualTo("Nice post!"));
            Assert.That(_post.Comments[0].Image, Is.Null);
        }

        [Test]
        public void AddComment_WithImageOnly_ShouldHaveNullContent()
        {
            var commentAuthor = Guid.NewGuid();
            var image = new byte[] { 1, 2, 3 };
            _post.AddComment(commentAuthor, null, image);

            Assert.That(_post.Comments.Count, Is.EqualTo(1));
            Assert.That(_post.Comments[0].Content, Is.Null);
            Assert.That(_post.Comments[0].Image, Is.EqualTo(image));
        }

        [Test]
        public void UpdatePost_ShouldChangeTitleContentAndImage()
        {
            var newTitle = "Updated Title";
            var newContent = "Updated Content";
            var newImage = new byte[] { 4, 5, 6 };

            _post.UpdatePost(newTitle, newContent, newImage);

            Assert.That(_post.Title, Is.EqualTo(newTitle));
            Assert.That(_post.Content, Is.EqualTo(newContent));
            Assert.That(_post.Image, Is.EqualTo(newImage));
        }

        [Test]
        public void UpdateComment_ShouldChangeContentAndImage()
        {
            var post = Post.CreateNewPost(Guid.NewGuid(), "Title", "Content", null);
            var comment = post.AddComment(Guid.NewGuid(), "Old Comment", null);

            var newContent = "New Comment";
            var newImage = new byte[] { 7, 8, 9 };
            comment.UpdateComment(newContent);
            comment.Image = newImage;

            Assert.That(comment.Content, Is.EqualTo(newContent));
            Assert.That(comment.Image, Is.EqualTo(newImage));
        }

        // --- DELETE ---
        [Test]
        public void RemoveComment_ShouldDeleteComment()
        {
            var commentAuthor = Guid.NewGuid();
            _post.AddComment(commentAuthor, "A comment", null);
            var commentId = _post.Comments[0].Id;

            _post.RemoveComment(commentId);

            Assert.That(_post.Comments.Count, Is.EqualTo(0));
        }
    }
}
