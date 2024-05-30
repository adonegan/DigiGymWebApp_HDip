using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Sockets;

namespace DigiGymWebApp_HDip.Controllers
{
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CalorieCounterService _calorieCounterService;

        public FoodController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CalorieCounterService calorieCounterService)
        {
            _context = context;
            _userManager = userManager;
            _calorieCounterService = calorieCounterService;
            _context.Database.EnsureCreated();
        }

        public async Task<ActionResult> FoodDiary()
        {
            var dates = await _context.FoodDiary
                .Select(f => f.CreatedAt.Date)
                .Distinct()
                .ToListAsync();
            return View(dates);
        }


        public async Task<ActionResult> Dates(DateTime date)
        {
            var foodEntry = await _context.FoodDiary
                    .Where(f => f.CreatedAt.Date == date.Date)
                    .ToListAsync();

            var totalCalories = await _calorieCounterService.GetTotalCalories(date);
            ViewBag.TotalCalories = totalCalories;

            return View(foodEntry);
        }



        public async Task<IActionResult> Create()
        {
            var enumMealTypeValues = Enum.GetValues(typeof(MealTypes));
            var selectListMealType = new SelectList(enumMealTypeValues);

            ViewBag.MealType = selectListMealType;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food food)
        {
            if (ModelState.IsValid)
            {
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
            var foodEntry = await _context.FoodDiary.FindAsync(id);
            var users = await _userManager.Users.ToListAsync();

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
                _context.Update(foodEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("FoodDiary");
            }
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var foodEntry = await _context.FoodDiary.FindAsync(id);

            return View(foodEntry);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var foodEntry = await _context.FoodDiary.FindAsync(id);

            _context.Remove(foodEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("FoodDiary");
        }

    }
}
