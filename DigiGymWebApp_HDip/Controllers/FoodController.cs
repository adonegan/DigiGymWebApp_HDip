using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Sockets;

namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "ClientOnly")]
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICalorieCounterService  _calorieCounterService;

        public FoodController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ICalorieCounterService  calorieCounterService)
        {
            _context = context;
            _userManager = userManager;
            _calorieCounterService = calorieCounterService;
            _context.Database.EnsureCreated();
        }

        public async Task<ActionResult> FoodDiary(string id)
        {
            var userId = _userManager.GetUserId(User);
            var dates = await _context.FoodDiary
                .Where(f => f.Id == userId)
                .Select(f => f.CreatedAt.Date)
                 // Show single date, even if date has multiple entries
                .Distinct()
                .ToListAsync();
            return View(dates);
        }

        public async Task<ActionResult> Dates(DateTime date)
        {
            var userId = _userManager.GetUserId(User);
            var foodEntry = await _context.FoodDiary
                    .Where(f => f.CreatedAt.Date == date.Date && f.Id == userId)
                    .ToListAsync();

            var totalCalories = await _calorieCounterService.GetTotalCalories(date, userId);
            ViewBag.TotalCalories = totalCalories;


            // get macro data for pie chart
            var totalProtein = foodEntry.Sum(m => m.Protein);
            var totalCarbs = foodEntry.Sum(m => m.Carbohydrates);
            var totalFat = foodEntry.Sum(m => m.Fat);

            ViewBag.TotalProtein = totalProtein;
            ViewBag.TotalCarbs = totalCarbs;
            ViewBag.TotalFat = totalFat;

            return View(foodEntry);
        }

        public async Task<IActionResult> Create()
        {
            var enumMealTypeValues = Enum.GetValues(typeof(MealTypes));
            var selectListMealType = new SelectList(enumMealTypeValues);

            ViewBag.MealType = selectListMealType;

            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food food)
        {
            if (ModelState.IsValid)
            {
                // Get user, string type
                var userId = _userManager.GetUserId(User);
                // Set Id property value to user's id
                food.Id = userId;
                food.CreatedAt = DateTime.Now;

                _context.Add(food);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirm", food);
            }
            return View();
        }

        public async Task<IActionResult> Confirm(Food food)
        {
            return View(food);
        }

        public async Task<IActionResult> Edit(int? id)
        {   
            var userId = _userManager.GetUserId(User);
            var foodEntry = await _context.FoodDiary
                                  .Where(f => f.FoodID == id && f.Id == userId)
                                   // Return first match
                                  .FirstOrDefaultAsync();

            var enumMealTypeValues = Enum.GetValues(typeof(MealTypes));
            var selectListMealType = new SelectList(enumMealTypeValues, foodEntry.MealType);

            ViewBag.MealType = selectListMealType;

            return View(foodEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FoodID,FoodName,FoodBrand,Service,Calories,Protein,Carbohydrates,Fat,MealType,Grams,CreatedAt")] Food foodEntry)
        {
            if (ModelState.IsValid)
            { 
                var userId = _userManager.GetUserId(User);
                var existingFoodEntry = await _context.FoodDiary
                                                  .Where(f => f.FoodID == id && f.Id == userId)
                                                   // Important
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

                foodEntry.Id = existingFoodEntry.Id;
                foodEntry.CreatedAt = existingFoodEntry.CreatedAt;

                _context.Update(foodEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("FoodDiary");
            }
            return View(foodEntry);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var foodEntry = await _context.FoodDiary
                                  .Where(f => f.FoodID == id && f.Id == userId)
                                  .FirstOrDefaultAsync();

            if (foodEntry == null)
            {
                return NotFound();
            }

            return View(foodEntry);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var foodEntry = await _context.FoodDiary
                                          .Where(f => f.FoodID == id && f.Id == userId)
                                          .FirstOrDefaultAsync();

            _context.Remove(foodEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("FoodDiary");
        }

    }
}
