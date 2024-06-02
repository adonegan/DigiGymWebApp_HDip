using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DigiGymWebApp_HDip.Models
{
    public enum UserTypes
    {
        Admin, Trainer, Client
    }

    public enum ApprovalStatuses
    {
        Pending, Approved, Rejected
    }

    public class ApplicationUser : IdentityUser 
    {
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public UserTypes UserType { get; set; }

        //Trainer approval
        [DisplayName("Approved?")]
        public ApprovalStatuses ApprovalStatus { get; set; }

        //Navigation properties
        public List<Workout> Workouts { get; set; }
        public List<Goal> Goals { get; set; }
        public List<Food> FoodDiary { get; set; }
        public List<Water> WaterEntries { get; set; }
        public List<UserProfile> ProfileEntries { get; set; }
    }

}
