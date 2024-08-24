using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DigiGymWebApp_HDip.Models
{
    public class Water
    {
        [Key]
        public int WaterID { get; set; }

        [Required]
        [DisplayName("Amount in mls")]
        public int Amount { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        //Foreign key
        public string Id { get; set; }

        //Navigation property
        public ApplicationUser User { get; set; }
    }
}
