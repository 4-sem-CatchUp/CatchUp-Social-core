namespace Social.Core
{
    public class ChatMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public Guid ChatId { get; set; }
        public Profile Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Seen { get; private set; }
        public readonly List<Image> _images = new();
        public IReadOnlyList<Image> Images => _images.AsReadOnly();

        public ChatMessage() { }

        public ChatMessage(Guid chatId, Profile sender, string content)
        {
            ChatId = chatId;
            Sender = sender;
            Content = content;
            Seen = false;
        }
        public void AddImage(string fileName, string contentType, byte[] data)
        {
            _images.Add(new Image(fileName, contentType, data));
        }

        public void MarkAsSeen()
        {
            Seen = true;
        }

        public void EditContent(string newContent)
        {
            Content = newContent;
            Timestamp = DateTime.UtcNow; // Update timestamp to reflect edit time
        }
    }
}
