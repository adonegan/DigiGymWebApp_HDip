using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DigiGymWebApp_HDip.Models
{
    public enum GoalTypes
    {
        Food, Water, Workout, Weight
    }

    public class Goal
    {
        [Key]
        public int GoalID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [DisplayName("Goal Type")]
        public GoalTypes GoalType { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string TargetValue { get; set; }

        [Required]
        public string CurrentValue { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Completion Date")]
        public DateTime? CompletionDate { get; set; }

        [Required]
        public bool IsAchieved { get; set; } = false;


        //Foreign key
        public string Id { get; set; }

        //Navigation property
        public ApplicationUser User { get; set; }
    }
}
