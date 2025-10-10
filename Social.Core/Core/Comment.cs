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

        public Comment() { }

        public Comment(Guid authorId, string text, DateTime timeStamp, List<Vote> votes)
        {
            AuthorId = authorId;
            Content = text;
            Timestamp = timeStamp;
            _votes = votes;
        }

        public static Comment CreateNewComment(Guid authorId, string text)
        {
            return new Comment { AuthorId = authorId, Content = text };
        }

        public void AddImage(string fileName, string contentType, byte[] data)
        {
            _images.Add(new Image(fileName, contentType, data));
        }

        // For use when reconstructing from DB
        public void AddImage(Image image)
        {
            _images.Add(image);
        }

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

        // For use when reconstructing from DB
        public void AddVote(Vote vote)
        {
            _votes.Add(vote);
        }

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
