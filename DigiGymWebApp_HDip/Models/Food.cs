using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DigiGymWebApp_HDip.Models
{
    public enum MealTypes
    {
        Breakfast, Lunch, Dinner, Snack
    }

    public class Food
    {
        [Key]
        public int FoodID { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Name")]
        public string FoodName { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Brand")]
        public string FoodBrand { get; set; }

        [Required]
        public int Serving { get; set; }

        [Required]
        public int Calories { get; set; }

        [Required]
        public int Protein { get; set; }

        [Required]
        public int Carbohydrates { get; set; }

        [Required]
        public int Fat { get; set; }

        [Required]
        public MealTypes MealType { get; set; }


        //Foreign key
        public string Id { get; set; }

        //Navigation property
        public ApplicationUser User { get; set; }
    }
}
