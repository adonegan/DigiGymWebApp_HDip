using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Services
{
    public interface IBMIService
    {
        Task<double> GetBMI(DateTime date, string userId);
    }

    public class BMIService : IBMIService
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
                                .FirstOrDefaultAsync();

            var height = profileEntry.Height;

            var weightEntry = await _context.WeightEntries
                                .Where(p => p.Id == userId)
                                .OrderByDescending(t => t.Timestamp)
                                .FirstOrDefaultAsync();

            // store weight values
            var weight = weightEntry.Weight;

            // convert lbs to kilograms
            double pounds2kilos = weight / 2.205;

            // formula: [weight (kg) / height (cm) / height (cm)] x 10,000
            return Math.Round(pounds2kilos / height / height* 10000, 1);
        }
    }
}




