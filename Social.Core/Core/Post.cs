namespace Social.Core
{
    public class Post
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid AuthorId { get; set; }

        public required string Title { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public int Karma => Votes.Sum(v => v.Upvote ? 1 : -1);

        private readonly List<Comment> _comments = new();

        public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();

        private readonly List<Vote> _votes = new();

        public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();

        private readonly List<Image> _images = new();

        public IReadOnlyList<Image> Images => _images.AsReadOnly();

        /// <summary>
        /// Creates a new post populated with default Title and Content values and sets CreatedAt to the current UTC time.
        /// </summary>
        public Post()
        {
            Title = "Nyt indlæg";
            Content = "Indhold kommer snart...";
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new Post with the specified author, title, content, and creation time and prepares empty comment and vote collections.
        /// </summary>
        /// <param name="authorId">The identifier of the post's author.</param>
        /// <param name="title">The post's title. Required.</param>
        /// <param name="content">The post's content, or null if none.</param>
        /// <param name="createdAt">The UTC date and time the post was created.</param>
        public Post(Guid authorId, string title, string? content, DateTime createdAt)
        {
            AuthorId = authorId;
            Title = title;
            Content = content;
            CreatedAt = createdAt;
            _votes = new List<Vote>();
            _comments = new List<Comment>();
        }

        /// <summary>
        /// Constructs a Post instance for reconstruction with the provided identity, metadata, and related collections.
        /// </summary>
        /// <param name="id">The unique identifier of the post.</param>
        /// <param name="authorId">The identifier of the post's author.</param>
        /// <param name="title">The post's title.</param>
        /// <param name="content">The post's content, or null if none.</param>
        /// <param name="image">An optional image provided for reconstruction; this constructor does not populate the post's Images collection from this parameter.</param>
        /// <param name="createdAt">The UTC creation timestamp to assign to the post.</param>
        /// <param name="comments">The list of Comment objects to use as the post's backing comments collection.</param>
        /// <param name="votes">The list of Vote objects to use as the post's backing votes collection.</param>
        public Post(
            Guid id,
            Guid authorId,
            string title,
            string? content,
            Image? image,
            DateTime createdAt,
            List<Comment> comments,
            List<Vote> votes
        )
        {
            Id = id;
            AuthorId = authorId;
            Title = title;
            Content = content;
            _votes = votes;
            CreatedAt = createdAt;
            _comments = comments;
        }

        /// <summary>
        /// Create a new Post with the specified author, title, and content and set CreatedAt to the current UTC time.
        /// </summary>
        /// <returns>The newly created Post with AuthorId, Title, Content populated and CreatedAt set to the current UTC time.</returns>
        public static Post CreateNewPost(Guid authorId, string title, string? content)
        {
            return new Post
            {
                AuthorId = authorId,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
            };
        }

        /// <summary>
        /// Associates a new Image with the post using the provided filename, content type, and binary data.
        /// </summary>
        /// <param name="fileName">The image file name.</param>
        /// <param name="contentType">The image MIME type (for example, "image/png").</param>
        /// <param name="data">The image binary data.</param>
        public void AddImage(string fileName, string contentType, byte[] data)
        {
            Image image = new Image(fileName, contentType, data);
            image.PostId = Id;
            _images.Add(image);
        }

        /// <summary>
        /// Adds an existing Image instance to the post's image collection (intended for DB reconstruction).
        /// </summary>
        /// <param name="image">The Image instance to append to the post's internal images list.</param>
        public void AddImage(Image image)
        {
            _images.Add(image);
        }

        /// <summary>
        /// Creates a new comment authored by the specified user and appends it to the post's comments.
        /// </summary>
        /// <param name="authorId">The identifier of the comment's author.</param>
        /// <param name="text">The comment text, or null for an empty comment.</param>
        /// <returns>The newly created <see cref="Comment"/>.</returns>
        public Comment AddComment(Guid authorId, string? text)
        {
            var comment = Comment.CreateNewComment(authorId, text);
            _comments.Add(comment);
            return comment;
        }

        /// <summary>
        /// Adds an existing Comment instance to the post's internal comments collection for reconstruction from persistent storage.
        /// </summary>
        /// <param name="comment">The Comment to append to the post's comments.</param>
        /// <returns>The same Comment instance that was added.</returns>
        public Comment AddComment(Comment comment)
        {
            _comments.Add(comment);
            return comment;
        }

        /// <summary>
        /// Adds, updates, or removes a vote by the specified user for this post.
        /// </summary>
        /// <param name="userId">The identifier of the user casting the vote.</param>
        /// <param name="upvote">`true` to cast an upvote, `false` to cast a downvote.</param>
        /// <returns>A <see cref="Vote"/> representing the vote that was added, updated, or removed.</returns>
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
        /// Adds an existing Vote to the post's votes collection for aggregate reconstruction.
        /// </summary>
        /// <param name="vote">The pre-existing Vote to append to this post's vote list.</param>
        public void AddVote(Vote vote)
        {
            _votes.Add(vote);
        }

        /// <summary>
        /// Gets the specified user's vote for this post.
        /// </summary>
        /// <param name="userId">The identifier of the user whose vote to retrieve.</param>
        /// <returns>`true` if the user upvoted the post, `false` if the user downvoted it, `null` if the user has not voted.</returns>
        public bool? GetUserVote(Guid userId)
        {
            var vote = Votes.FirstOrDefault(v => v.UserId == userId);
            return vote?.Upvote;
        }

        /// <summary>
        /// Updates the post's title and content when new values are provided.
        /// </summary>
        /// <param name="newTitle">The new title to set; if null the existing title is not changed.</param>
        /// <param name="newContent">The new content to set; if null the existing content is not changed.</param>
        public void UpdatePost(string? newTitle, string? newContent)
        {
            if (newTitle != null)
                Title = newTitle;
            if (newContent != null)
                Content = newContent;
        }

        /// <summary>
        /// Removes the comment with the specified identifier from the post's comment collection if it exists.
        /// </summary>
        /// <param name="commentId">The identifier of the comment to remove.</param>
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