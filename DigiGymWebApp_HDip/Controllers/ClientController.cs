using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "ClientOnly")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>_roleManager;
        private readonly BMIService _bmiService;
        private readonly BMICategory _bmiCategory;

        public ClientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, BMIService bmiService, BMICategory bmiCategory)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _bmiService = bmiService;
            _bmiCategory = bmiCategory;
            _context.Database.EnsureCreated();
        }

        public async Task<IActionResult> Create()
        {
            var enumGenderTypeValues = Enum.GetValues(typeof(GenderTypes));
            var selectListGenderType = new SelectList(enumGenderTypeValues);

            ViewBag.GenderType = selectListGenderType;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserProfile profile)
        {
            if (ModelState.IsValid)
            {
                // Attribute user to record
                var userId = _userManager.GetUserId(User);
                profile.Id = userId;
                profile.Timestamp = DateTime.Now;

                _context.Add(profile);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirm", profile);
            }
            return View();
        }

        public async Task<IActionResult> Confirm(UserProfile profile)
        {
            return View(profile);
        }


        public async Task<IActionResult> Edit(int? id)
        {   
            var userId = _userManager.GetUserId(User);
            var profileEntry = await _context.ProfileEntries
                                  .Where(p => p.ProfileID == id && p.Id == userId)
                                  .FirstOrDefaultAsync();

            var enumGenderTypeValues = Enum.GetValues(typeof(GenderTypes));
            var selectListGenderType = new SelectList(enumGenderTypeValues, profileEntry.Gender);

            ViewBag.Gender = selectListGenderType;

            return View(profileEntry);
        }
            
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProfileID,Weight,Height,Gender,Timestamp")] UserProfile profileEntry)
        {
            if (ModelState.IsValid)
            { 
                var userId = _userManager.GetUserId(User);
                var existingProfileEntry = await _context.ProfileEntries
                                                  .Where(p => p.ProfileID == id && p.Id == userId)
                                                   // Important
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

                profileEntry.Id = existingProfileEntry.Id;
                profileEntry.Timestamp = profileEntry.Timestamp;

                _context.Update(profileEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("Profile");
            }
            return View();
        }


        public async Task<IActionResult> Profile(DateTime date)
        {
            var userId = _userManager.GetUserId(User);
            var profileEntry = await _context.ProfileEntries
                                         // Leverage navigation property in View
                                        .Include(p => p.User)
                                        .Where(p => p.Id == userId)
                                        .OrderByDescending(p => p.Timestamp)
                                        .FirstOrDefaultAsync();

            // make BMI value available
            var bmiService = await _bmiService.GetBMI(date, userId);
            ViewBag.BMIService = bmiService;

            var bmiCategory = await _bmiCategory.GetBMICategory(bmiService);
            ViewBag.BMICategory = bmiCategory;

            return View(profileEntry);
        }
    }
}
