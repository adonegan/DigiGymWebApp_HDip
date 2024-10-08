﻿using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Services
{
    // implement interface instead of concrete class for testing purposes
    // prevent parameterless constructor error during testing
    public interface ICalorieCounterService
    {
        Task<int> GetTotalCalories(DateTime date, string userId);
    }

    public class CalorieCounterService : ICalorieCounterService
    {
        private readonly ApplicationDbContext _context;

        public CalorieCounterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCalories(DateTime date, string userId)
        {
            // User not available directly inside method outside controller, so get it in controller and pass via parameter instead
            var foodEntriesByDate = await _context.FoodDiary
                                        .Where(f => f.CreatedAt.Date == date.Date && f.Id == userId)
                                        .ToListAsync();

            return foodEntriesByDate.Sum(f => f.Calories);
        }

    }
}
