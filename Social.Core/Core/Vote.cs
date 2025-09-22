namespace Social.Core
{
    public class Vote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TargetId { get; set; }
        public VoteTargetType VoteTargetType { get; set; }
        public Guid UserId { get; set; }
        public bool Upvote { get; set; }
        public VoteAction Action { get; set; } = VoteAction.Add;
    }

    public enum VoteTargetType
    {
        Post,
        Comment,
    }

    public enum VoteAction
    {
        Add,
        Remove,
        Update,
    }
}
