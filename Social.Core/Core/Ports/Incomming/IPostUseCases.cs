namespace Social.Core.Ports.Incomming
{
    public interface IPostUseCases
    {
        /// <summary>
        /// Creates a new post and returns the created post's identifier.
        /// </summary>
        /// <param name="authorId">Identifier of the post author.</param>
        /// <param name="title">Title of the post.</param>
        /// <param name="content">Optional body content of the post; may be null or empty.</param>
        /// <param name="images">Optional list of images to attach to the post; may be null or empty.</param>
        /// <returns>The identifier of the newly created post.</returns>
        Task<Guid> CreatePostAsync(
            Guid authorId,
            string title,
            string? content,
            List<Image>? images
        );
        /// <summary>
/// Registers a vote by a user on a post.
/// </summary>
/// <param name="postId">Identifier of the post to vote on.</param>
/// <param name="upVote">Whether the vote is an upvote (`true`) or a downvote (`false`).</param>
/// <param name="userId">Identifier of the user casting the vote.</param>
public Task VotePost(Guid postId, bool upVote, Guid userId);
        /// <summary>
/// Retrieves the vote state that a specific user cast on a post.
/// </summary>
/// <param name="postId">The identifier of the post.</param>
/// <param name="userId">The identifier of the user.</param>
/// <returns>`true` if the user upvoted the post, `false` if the user downvoted the post, `null` if the user has not voted.</returns>
public Task<bool?> GetUserPostVote(Guid postId, Guid userId);
        /// <summary>
/// Updates the title and/or content of an existing post.
/// </summary>
/// <param name="postId">Identifier of the post to update.</param>
/// <param name="newTitle">New title to apply; if null the post's title is left unchanged.</param>
/// <param name="newContent">New content to apply; if null the post's content is left unchanged.</param>
public Task UpdatePostAsync(Guid postId, string? newTitle, string? newContent);
        /// <summary>
/// Deletes the post with the specified identifier.
/// </summary>
/// <param name="postId">Identifier of the post to delete.</param>
public Task DeletePost(Guid postId);
    }
}