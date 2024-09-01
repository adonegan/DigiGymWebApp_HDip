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

            // for newly registered users who have no records in table yet
            if (profileEntry == null)
            {
                var defaultHeight = new UserProfile
                {
                    Height = 0,
                    Gender = GenderTypes.None,
                    Id = userId
                };
                _context.ProfileEntries.Add(defaultHeight);
                 await _context.SaveChangesAsync();

                profileEntry = defaultHeight;
            } 
           
            var weightEntry = await _context.WeightEntries
                                .Where(p => p.Id == userId)
                                .OrderByDescending(t => t.Timestamp)
                                .FirstOrDefaultAsync();

            // for newly registered users who have no records in table yet
            if (weightEntry == null)
            {
                var defaultWeight = new WeightEntry
                {
                    Weight = 0,
                    Timestamp = DateTime.Now,
                    Id = userId
                };
                _context.WeightEntries.Add(defaultWeight);
                await _context.SaveChangesAsync();

                weightEntry = defaultWeight;
            } 
            
            var height = profileEntry.Height;
            var weight = weightEntry.Weight;

            // fi height / weight is 0 return 0
            if (profileEntry.Height == 0 || weightEntry.Weight == 0)
            {
                return 0;
            }

            // convert lbs to kilograms
            double pounds2kilos = weight / 2.205;

            // formula: [weight (kg) / height (cm) / height (cm)] x 10,000
            return Math.Round(pounds2kilos / height / height* 10000, 1);
        }
    }
}




