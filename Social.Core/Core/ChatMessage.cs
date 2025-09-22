namespace Social.Core
{
    public class ChatMessage
    {
        public Guid MessageId { get; set; }
        public Profile Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Seen { get; private set; }
        public ChatMessage() { }
        public ChatMessage(Profile sender, string content)
        {
            MessageId = Guid.NewGuid();
            Sender = sender;
            Content = content;
            Timestamp = DateTime.UtcNow;
            Seen = false;
        }
        public void MarkAsSeen()
        {
            Seen = true;
        }
    }
}
