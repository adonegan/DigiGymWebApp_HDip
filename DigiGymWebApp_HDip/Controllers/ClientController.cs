using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "ClientOnly")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>_roleManager;
        private readonly IBMIService _bmiService;
        private readonly IBMICategory _bmiCategory;

        public ClientController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IBMIService bmiService, IBMICategory bmiCategory)
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

            return View("Create");
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
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            var enumGenderTypeValues = Enum.GetValues(typeof(GenderTypes));
            var selectListGenderType = new SelectList(enumGenderTypeValues, profileEntry.Gender);

            ViewBag.Gender = selectListGenderType;

            return View(profileEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProfileID,Height,Gender")] UserProfile profileEntry)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var existingProfileEntry = await _context.ProfileEntries
                                                  .Where(p => p.ProfileID == id && p.Id == userId)
                                                  .FirstOrDefaultAsync();

                if (existingProfileEntry == null)
                {
                    return NotFound();
                }

                existingProfileEntry.Height = profileEntry.Height;
                existingProfileEntry.Gender = profileEntry.Gender;
                
                _context.Update(existingProfileEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("Profile");
            }

            // If ModelState not valid, handle ViewBag properties
            var enumGenderTypeValues = Enum.GetValues(typeof(GenderTypes));
            var selectListGenderType = new SelectList(enumGenderTypeValues, profileEntry.Gender);
            ViewBag.Gender = selectListGenderType;

            return View(profileEntry);
        }

        public async Task<IActionResult> Profile(DateTime date)
        {
            var userId = _userManager.GetUserId(User);
            var profileEntry = await _context.ProfileEntries
                                        // Leverage navigation property in View
                                        .Include(p => p.User)
                                        .Where(p => p.Id == userId)
                                        .FirstOrDefaultAsync();

            // get latest weight entry
            var weightEntry = await _context.WeightEntries
                                        .Include(p => p.User)
                                        .Where (p => p.Id == userId)
                                        .OrderByDescending(w => w.Timestamp) 
                                        .FirstOrDefaultAsync();

            ViewBag.WeightEntry = weightEntry;

            // make BMI value available
            var bmiService = await _bmiService.GetBMI(date, userId);
            ViewBag.BMIService = bmiService;

            var bmiCategory = await _bmiCategory.GetBMICategory(bmiService);
            ViewBag.BMICategory = bmiCategory;

            return View(profileEntry);
        }
    }
}
