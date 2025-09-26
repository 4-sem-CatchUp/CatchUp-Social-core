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
    }
}
