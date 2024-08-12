namespace DigiGymWebApp_HDip.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }

        public Conversation Conversation { get; set; }
        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}
