using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Services
{
    public class BMIService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BMIService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<double> GetBMI(DateTime date, string userId)
        {
            // get latest profile entry, order by timestamp
            var profileEntry = await _context.ProfileEntries
                                .Where(p => p.Id == userId)
                                .OrderByDescending(p => p.Timestamp)
                                .FirstOrDefaultAsync();

            // store weight and height values
            var weight = profileEntry.Weight;
            var height = profileEntry.Height;

            // convert lbs to kilograms
            double pounds2kilos = weight / 2.205;

            // formula: [weight (kg) / height (cm) / height (cm)] x 10,000
            return Math.Round(pounds2kilos / height / height * 10000, 1);

        }
    }
}




