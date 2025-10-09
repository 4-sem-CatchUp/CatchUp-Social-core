using System.ComponentModel.DataAnnotations;

namespace Social.Core
{
    public class Comment
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid AuthorId { get; private set; }

        public string? Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int Karma => Votes.Sum(v => v.Upvote ? 1 : -1);

        private readonly List<Vote> _votes = new();
        public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();

        private readonly List<Image> _images = new();
        public IReadOnlyList<Image> Images => _images.AsReadOnly();

        /// <summary>
/// Initializes a new Comment instance with default values.
/// </summary>
/// <remarks>
/// The new comment is assigned a new Id, Timestamp is set to DateTime.UtcNow, and internal collections for votes and images are initialized empty. Content is null by default.
/// </remarks>
public Comment() { }

        /// <summary>
        /// Initialize a new Comment with the specified author, content, timestamp, and existing votes (used for reconstruction).
        /// </summary>
        /// <param name="authorId">The identifier of the comment's author.</param>
        /// <param name="text">The comment text content.</param>
        /// <param name="timeStamp">The creation timestamp for the comment.</param>
        /// <param name="votes">A pre-populated list of votes to use as the comment's backing vote collection.</param>
        public Comment(Guid authorId, string text, DateTime timeStamp, List<Vote> votes)
        {
            AuthorId = authorId;
            Content = text;
            Timestamp = timeStamp;
            _votes = votes;
        }

        /// <summary>
        /// Create a new Comment with the specified author and content.
        /// </summary>
        /// <returns>A Comment whose AuthorId and Content are set to the provided values; other properties use their defaults.</returns>
        public static Comment CreateNewComment(Guid authorId, string text)
        {
            return new Comment { AuthorId = authorId, Content = text };
        }

        /// <summary>
        /// Adds a new image to the comment's images collection.
        /// </summary>
        /// <param name="fileName">The original file name for the image.</param>
        /// <param name="contentType">The image MIME type (for example, "image/png").</param>
        /// <param name="data">The image binary data.</param>
        public void AddImage(string fileName, string contentType, byte[] data)
        {
            _images.Add(new Image(fileName, contentType, data));
        }

        /// <summary>
        /// Adds an Image to the comment's image collection.
        /// </summary>
        /// <param name="image">The Image instance to attach to this comment (used when reconstructing from storage).</param>
        public void AddImage(Image image)
        {
            _images.Add(image);
        }

        /// <summary>
        /// Adds, updates, or removes a user's vote on the comment according to the requested vote value.
        /// </summary>
        /// <param name="userId">The identifier of the user casting the vote.</param>
        /// <param name="upvote">`true` to cast an upvote, `false` to cast a downvote.</param>
        /// <returns>The Vote representing the user's final vote state; its Action indicates whether the vote was added, updated, or removed.</returns>
        public Vote AddVote(Guid userId, bool upvote)
        {
            var vote = Votes.FirstOrDefault(v => v.UserId == userId);
            if (vote != null)
            {
                if (vote.Upvote != upvote)
                {
                    vote.Upvote = upvote;
                    vote.Action = VoteAction.Update;
                }
                else if (vote.Upvote == upvote)
                {
                    _votes.Remove(vote);
                    vote.Action = VoteAction.Remove;
                }
            }
            else
            {
                vote = new Vote
                {
                    Id = Guid.NewGuid(),
                    TargetId = Id,
                    VoteTargetType = VoteTargetType.Post,
                    UserId = userId,
                    Upvote = upvote,
                    Action = VoteAction.Add,
                };
                _votes.Add(vote);
            }
            return vote;
        }

        /// <summary>
        /// Adds an existing Vote to this comment's vote collection.
        /// </summary>
        /// <param name="vote">The Vote instance to add (used when rehydrating from storage).</param>
        public void AddVote(Vote vote)
        {
            _votes.Add(vote);
        }

        /// <summary>
        /// Updates the comment's text content.
        /// </summary>
        /// <param name="newText">The replacement text for the comment's Content property.</param>
        public void UpdateComment(string newText)
        {
            Content = newText;
        }

        public bool? GetUserVote(Guid userId)
        {
            var vote = Votes.FirstOrDefault(v => v.UserId == userId);
            return vote?.Upvote;
        }
    }
}