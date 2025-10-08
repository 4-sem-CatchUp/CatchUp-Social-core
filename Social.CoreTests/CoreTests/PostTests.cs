using Social.Core;

namespace SocialCoreTests.CoreTests
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
            Assert.That(_post.Images.Count, Is.EqualTo(0));
            Assert.That(_post.Votes.Count, Is.EqualTo(0));
            Assert.That(_post.Comments.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddImage_ShouldStoreImageInPost()
        {
            var data = new byte[] { 1, 2, 3 };
            _post.AddImage("test.png", "image/png", data);

            Assert.That(_post.Images.Count, Is.EqualTo(1));
            Assert.That(_post.Images[0].FileName, Is.EqualTo("test.png"));
            Assert.That(_post.Images[0].ContentType, Is.EqualTo("image/png"));
            Assert.That(_post.Images[0].Data, Is.EqualTo(data));
        }

        // --- COMMENTS ---
        [Test]
        public void AddComment_ShouldAddComment()
        {
            var commentAuthor = Guid.NewGuid();
            _post.AddComment(commentAuthor, "Nice post!");

            Assert.That(_post.Comments.Count, Is.EqualTo(1));
            Assert.That(_post.Comments[0].AuthorId, Is.EqualTo(commentAuthor));
            Assert.That(_post.Comments[0].Content, Is.EqualTo("Nice post!"));
            Assert.That(_post.Comments[0].Images.Count, Is.EqualTo(0));
        }

        [Test]
        public void Comment_AddImage_ShouldStoreImage()
        {
            var commentAuthor = Guid.NewGuid();
            var comment = _post.AddComment(commentAuthor, "with image");
            comment.AddImage("c.png", "image/png", new byte[] { 9 });

            Assert.That(comment.Images.Count, Is.EqualTo(1));
            Assert.That(comment.Images[0].FileName, Is.EqualTo("c.png"));
            Assert.That(comment.Images[0].Data, Is.EqualTo(new byte[] { 9 }));
        }

        [Test]
        public void UpdateComment_ShouldChangeContent()
        {
            var comment = _post.AddComment(Guid.NewGuid(), "Old text");
            comment.UpdateComment("New text");

            Assert.That(comment.Content, Is.EqualTo("New text"));
        }

        [Test]
        public void RemoveComment_ShouldDeleteComment()
        {
            var comment = _post.AddComment(Guid.NewGuid(), "to be deleted");
            _post.RemoveComment(comment.Id);

            Assert.That(_post.Comments.Count, Is.EqualTo(0));
        }

        // --- VOTES ---
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
        public void GetUserVote_ShouldReturnCorrectVote()
        {
            var userId = Guid.NewGuid();
            _post.AddVote(userId, true);

            var vote = _post.GetUserVote(userId);
            Assert.That(vote.HasValue && vote.Value, Is.True);
        }

        [Test]
        public void GetUserVote_ShouldReturnNull_WhenNoVoteExists()
        {
            var vote = _post.GetUserVote(Guid.NewGuid());
            Assert.That(vote, Is.Null);
        }

        [Test]
        public void Comment_AddVote_ShouldAffectKarma()
        {
            var comment = _post.AddComment(Guid.NewGuid(), "Comment");
            var userId = Guid.NewGuid();

            comment.AddVote(userId, true);
            Assert.That(comment.Votes.Count, Is.EqualTo(1));
            Assert.That(comment.Karma, Is.EqualTo(1));

            comment.AddVote(userId, false);
            Assert.That(comment.Votes.Count, Is.EqualTo(1));
            Assert.That(comment.Karma, Is.EqualTo(-1));
        }

        // --- UPDATE POST ---
        [Test]
        public void UpdatePost_ShouldChangeTitleAndContent()
        {
            _post.UpdatePost("Updated title", "Updated content");

            Assert.That(_post.Title, Is.EqualTo("Updated title"));
            Assert.That(_post.Content, Is.EqualTo("Updated content"));
        }

        // --- MULTIPLE ---
        [Test]
        public void AddMultipleImages_ShouldStoreAllImages()
        {
            _post.AddImage("1.png", "image/png", new byte[] { 1 });
            _post.AddImage("2.jpg", "image/jpeg", new byte[] { 2 });

            Assert.That(_post.Images.Count, Is.EqualTo(2));
            Assert.That(_post.Images.Any(i => i.FileName == "1.png"));
            Assert.That(_post.Images.Any(i => i.FileName == "2.jpg"));
        }
    }
}
