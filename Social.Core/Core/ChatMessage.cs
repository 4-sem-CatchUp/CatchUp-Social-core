namespace Social.Core
{
    public class ChatMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public Guid ChatId { get; set; }
        public Profile Sender { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Seen { get; private set; }
        public Image? Image { get; private set; }

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
            Image = new Image(fileName, contentType, data);
        }

        // For use when reconstructing from DB
        public void AddImage(Image image)
        {
            Image = image;
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
