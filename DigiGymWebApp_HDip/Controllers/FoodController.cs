using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace DigiGymWebApp_HDip.Controllers
{
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FoodController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _context.Database.EnsureCreated();
        }

        public async Task<IActionResult> FoodDiary()
        {
            var foodDiary = await _context.FoodDiary.ToListAsync();
            return View(foodDiary);
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

    }
}
