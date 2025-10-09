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

        /// <summary>
/// Initializes a new ChatMessage with default values.
/// </summary>
/// <remarks>
/// Creates an empty chat message whose MessageId is initialized to a new GUID and whose Timestamp is set to the current UTC time. Content and Image are null and Seen is false by default.
/// </remarks>
public ChatMessage() { }

        /// <summary>
        /// Initialize a new chat message for the specified chat, sender, and content.
        /// The message is created with its Seen flag set to false.
        /// </summary>
        /// <param name="chatId">Identifier of the chat this message belongs to.</param>
        /// <param name="sender">Profile of the message sender.</param>
        /// <param name="content">Message text; may be null to represent an empty message.</param>
        public ChatMessage(Guid chatId, Profile sender, string content)
        {
            ChatId = chatId;
            Sender = sender;
            Content = content;
            Seen = false;
        }

        /// <summary>
        /// Attaches a new Image to the chat message from raw file data, replacing any existing image.
        /// </summary>
        /// <param name="fileName">The original file name for the image.</param>
        /// <param name="contentType">The MIME content type of the image (e.g., "image/png").</param>
        /// <param name="data">The binary image data.</param>
        public void AddImage(string fileName, string contentType, byte[] data)
        {
            Image = new Image(fileName, contentType, data);
        }

        /// <summary>
        /// Attaches an existing Image to the chat message, typically used when reconstructing the message from persistent storage.
        /// </summary>
        /// <param name="image">The Image to associate with this message.</param>
        public void AddImage(Image image)
        {
            Image = image;
        }

        /// <summary>
        /// Marks the chat message as seen.
        /// </summary>
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