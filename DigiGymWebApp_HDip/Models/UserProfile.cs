using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DigiGymWebApp_HDip.Models
{
    public enum GenderTypes
    {
        Male, Female, None
    }

    public class UserProfile
    {
        [Key]
        public int ProfileID { get; set; }

        [Required]
        [DisplayName("Height (in meters)")]
        [Range(1.2, double.MaxValue, ErrorMessage = "Must be above 1.2 meters!")]
        public double Height { get; set; }

        public GenderTypes Gender { get; set; }

        public string Id { get; set; }

        public ApplicationUser User { get; set; }

    }
}