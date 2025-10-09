namespace Social.Core
{
    public class Image
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();

        // Navigation
        public Guid? PostId { get; set; }
        public Post? Post { get; set; }

        public Guid? CommentId { get; set; }
        public Comment Comment { get; set; }

        public Guid? ProfileId { get; set; }
        public Profile? Profile { get; set; }

        public Guid? ChatMessageId { get; set; }
        public ChatMessage? ChatMessage { get; set; }

        /// <summary>
/// Initializes a new Image instance with default property values.
/// </summary>
public Image() { }

        /// <summary>
        /// Creates a new Image initialized with the specified file name, content type, and binary data.
        /// </summary>
        /// <param name="fileName">The image file name, typically including extension (e.g., "photo.jpg").</param>
        /// <param name="contentType">The MIME content type of the image (e.g., "image/jpeg").</param>
        /// <param name="data">The raw binary data of the image file.</param>
        public Image(string fileName, string contentType, byte[] data)
        {
            FileName = fileName;
            ContentType = contentType;
            Data = data;
        }

        /// <summary>
        /// Initializes a new Image instance by copying values from the provided source image.
        /// </summary>
        /// <param name="image">The source Image whose scalar properties and, conditionally, navigation relationships are copied.</param>
        /// <remarks>
        /// Scalar properties (Id, FileName, ContentType, Data) are copied unconditionally. For each navigation relationship (Post, Comment, Profile, ChatMessage), the corresponding Id and navigation reference are copied from <paramref name="image"/> only if this instance already has that relationship's Id set.
        /// </remarks>
        public Image(Image image)
        {
            Id = image.Id;
            FileName = image.FileName;
            ContentType = image.ContentType;
            Data = image.Data;
            if (PostId != null)
            {
                PostId = image.PostId;
                Post = image.Post;
            }
            if (CommentId != null)
            {
                CommentId = image.CommentId;
                Comment = image.Comment;
            }
            if (ProfileId != null)
            {
                ProfileId = image.ProfileId;
                Profile = image.Profile;
            }
            if (ChatMessageId != null)
            {
                ChatMessageId = image.ChatMessageId;
                ChatMessage = image.ChatMessage;
            }
        }
    }
}