namespace Social.Core
{
    public class Post
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid AuthorId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        private readonly List<Comment> _comments = new();

        public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();

        private readonly List<Vote> _votes = new();

        public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();

        private int _karma;
        public int Karma
        {
            get
            {
                _karma = 0;
                foreach (var vote in Votes)
                {
                    _karma += vote.Upvote ? 1 : -1;
                }
                return _karma;
            }
        }
        public Post() { }
        public Post(Guid authorId,
                    string title,
                    string content,
                    DateTime createdAt)
        {
            AuthorId = authorId;
            Title = title;
            Content = content;
            CreatedAt = createdAt;
            _votes = new List<Vote>();
            _comments = new List<Comment>();
        }

        public Post(Guid id,
                    Guid authorId,
                    string title,
                    string content,
                    DateTime createdAt,
                    List<Comment> comments,
                    List<Vote> votes)
        {
            Id = id;
            AuthorId = authorId;
            Title = title;
            Content = content;
            _votes = votes;
            CreatedAt = createdAt;
            _comments = comments;
        }

        public static Post CreateNewPost(Guid authorId, string title, string content)
        {
            return new Post
            {
                AuthorId = authorId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
            };
        }
        public Comment AddComment(Guid authorId, string text)
        {
            var comment = Comment.CreateNewComment(authorId, text);
            _comments.Add(comment);
            return comment;

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
                    Action = VoteAction.Add
                };
                _votes.Add(vote);
            }
            return vote;
        }

        public bool? GetUserVote(Guid userId)
        {
            var vote = Votes.FirstOrDefault(v => v.UserId == userId);
            return vote?.Upvote;
        }

        public void UpdatePost(string? newTitle, string? newContent)
        {
            if (newTitle != null)
                Title = newTitle;
            if (newContent != null)
                Content = newContent;
        }
        public void RemoveComment(Guid commentId)
        {
            var comment = Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment != null)
            {
                _comments.Remove(comment);
            }
        }
    }
}
