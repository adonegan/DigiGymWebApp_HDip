using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Services
{
    public class CalorieCounterService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalorieCounterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCalories(DateTime date)
        {
            var foodEntriesByDate = await _context.FoodDiary
                                        .Where(f => f.CreatedAt.Date == date.Date)
                                        .ToListAsync();

            return foodEntriesByDate.Sum(f => f.Calories);
        }

    }
}
