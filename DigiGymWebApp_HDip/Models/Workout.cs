using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DigiGymWebApp_HDip.Models
{
    public enum WorkoutTypes
    {
        Walk, Run, Cycle, Swim, Other
    }

    public enum EffortLevels
    {
        Low, Medium, High, Peak
    }

    public class Workout
    {
        [Key]
        public int WorkoutID { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        [DisplayName("Workout Type")]
        public WorkoutTypes WorkoutType { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }

        [Required]
        [DisplayName("Effort Level")]
        public EffortLevels EffortLevel { get; set; }


        //Foreign key
        public string Id { get; set; }

        //Navigation property
        public ApplicationUser User { get; set; }
    }
}
