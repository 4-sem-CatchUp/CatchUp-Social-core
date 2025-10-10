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

        public Image() { }

        public Image(string fileName, string contentType, byte[] data)
        {
            FileName = fileName;
            ContentType = contentType;
            Data = data;
        }

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
