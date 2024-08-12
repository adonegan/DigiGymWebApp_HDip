namespace DigiGymWebApp_HDip.Models
{
    public class Conversation
    {
        public int ConversationID { get; set; }
        public string ClientID { get; set; }
        public string TrainerID { get; set; }

        public ApplicationUser Client { get; set; }
        public ApplicationUser Trainer { get; set; }
        public List<Message> Messages { get; set; }
    }
}