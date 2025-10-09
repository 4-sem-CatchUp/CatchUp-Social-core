namespace Social.Core.Ports.Outgoing
{
    public interface IImageRepository
    {
        /// <summary>
/// Adds a new image to the repository.
/// </summary>
/// <param name="image">The image to add.</param>
Task AddAsync(Image image);
        /// <summary>
/// Deletes the image with the specified identifier.
/// </summary>
/// <param name="imageId">The unique identifier of the image to remove.</param>
Task DeleteAsync(Guid imageId);
        /// <summary>
/// Retrieves an image by its unique identifier.
/// </summary>
/// <param name="imageId">The unique identifier of the image to retrieve.</param>
/// <returns>The image with the specified identifier, or null if no image exists with that identifier.</returns>
Task<Image?> GetByIdAsync(Guid imageId);

        /// <summary>
/// Retrieves all images associated with the specified comment.
/// </summary>
/// <param name="commentId">The unique identifier of the comment whose images to retrieve.</param>
/// <returns>A read-only list of images linked to the comment; an empty list if no images are found.</returns>
Task<IReadOnlyList<Image>> GetByCommentIdAsync(Guid commentId);
        /// <summary>
/// Retrieves all images attached to the specified post.
/// </summary>
/// <param name="postId">The unique identifier of the post.</param>
/// <returns>A read-only list of images associated with the post (empty if none).</returns>
Task<IReadOnlyList<Image>> GetByPostIdAsync(Guid postId);
        /// <summary>
/// Retrieves the image associated with the specified profile.
/// </summary>
/// <param name="profileId">The unique identifier of the profile whose image to retrieve.</param>
/// <returns>The profile's <see cref="Image"/>.</returns>
Task<Image> GetByProfileIdAsync(Guid profileId);
        /// <summary>
/// Retrieves all images attached to a specific chat message.
/// </summary>
/// <param name="chatMessageId">The unique identifier of the chat message.</param>
/// <returns>A read-only list of Image objects associated with the chat message; empty if none exist.</returns>
Task<IReadOnlyList<Image>> GetByChatMessageIdAsync(Guid chatMessageId);
    }
}