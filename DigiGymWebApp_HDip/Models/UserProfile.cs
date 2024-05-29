using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DigiGymWebApp_HDip.Models
{

    public class UserProfile
    {
        [Key]
        public int ProfileID { get; set; }

        [Required]
        [DisplayName("Weight (in lbs)")]
        [Range(70, double.MaxValue, ErrorMessage = "Must be above 70 lbs")]
        public double Weight { get; set; }

        [Required]
        [DisplayName("Height (in meteres)")]
        [Range(1.2, Double.MaxValue, ErrorMessage = "Must be above 1.2 meters!")]
        public double Height { get; set; }

        public double BMIValue { get; set; }

        public string BMICategory { get; set; }

        public string Id { get; set; }

        public ApplicationUser User { get; set; }

    }
}