using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Services
{
    public interface IBMICategory
    {
        Task<string> GetBMICategory(double BMI);
    }

    public class BMICategory : IBMICategory
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BMICategory(ApplicationDbContext context)
        {
            _context = context;
        }

        public static string[] BMICategories
        {
            get
            {
                return new string[] {"Underweight", "Normal", "Overweight", "Obese"};
            }
        }

        public async Task<string> GetBMICategory(double BMI)
        {
            if (BMI <= 18.5)
            {
                return BMICategories[0];
            }
            else if (BMI >= 18.5 && BMI < 24.9)
            {
                return BMICategories[1];
            }
            else if (BMI > 25 && BMI <= 29.9)
            {
                return BMICategories[2];
            }
            else if (BMI > 30)
            {
                return BMICategories[3];
            }
            else
            {
                throw new Exception("Out of range");
            }
        }
    }
}








