namespace Social.Core.Ports.Incomming
{
    public interface ICommentUseCases
    {
        /// <summary>
/// Adds a comment to the specified post.
/// </summary>
/// <param name="postId">The identifier of the post to which the comment will be added.</param>
/// <param name="authorId">The identifier of the user creating the comment.</param>
/// <param name="text">Optional text content of the comment.</param>
/// <param name="images">Optional list of images to attach to the comment.</param>
Task AddComment(Guid postId, Guid authorId, string? text, List<Image>? images);
        /// <summary>
/// Record a user's vote on a comment.
/// </summary>
/// <param name="commentId">Identifier of the comment to vote on.</param>
/// <param name="upVote">`true` to register an upvote, `false` to register a downvote.</param>
/// <param name="userId">Identifier of the user casting the vote.</param>
public Task VoteComment(Guid commentId, bool upVote, Guid userId);
        /// <summary>
/// Gets the voting state of a user for a specific comment within a post.
/// </summary>
/// <returns>`true` if the user upvoted the comment, `false` if the user downvoted the comment, `null` if the user has not voted or the vote is unavailable.</returns>
public Task<bool?> GetUserCommentVote(Guid postId, Guid commentId, Guid userId);
        /// <summary>
/// Updates the text content of the specified comment when the requesting user is authorized to modify it.
/// </summary>
/// <param name="commentId">Identifier of the comment to update.</param>
/// <param name="userId">Identifier of the user attempting the update.</param>
/// <param name="newContent">The new text content for the comment.</param>
/// <returns>`true` if the update succeeded, `false` if the update failed, or `null` if the update is not applicable or unavailable.</returns>
public Task<bool?> UpdateCommentAsync(Guid commentId, Guid userId, string newContent);
        public Task<bool?> DeleteComment(Guid postId, Guid commentId, Guid userId);
    }
}