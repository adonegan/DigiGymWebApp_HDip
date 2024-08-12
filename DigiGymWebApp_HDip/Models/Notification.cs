namespace DigiGymWebApp_HDip.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime Timestamp { get; set; }

        public ApplicationUser User { get; set; }
    }
}
