using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DigiGymWebApp_HDip.Models
{
    public class WeightEntry
    {
        [Key]
        public int WeightID { get; set; }

        [Required]
        [DisplayName("Weight (in lbs)")]
        [Range(70, double.MaxValue, ErrorMessage = "Must be above 70 lbs")]
        public double Weight { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        //Foreign key
        public string Id { get; set; }

        //Navigation property
        public ApplicationUser User { get; set; }
    }
}
