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
    public class WeightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WeightController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _context.Database.EnsureCreated();
        }


        public async Task<IActionResult> WeightEntries()
        {
            var weightEntries = await _context.WeightEntries
                                              .OrderBy(w => w.Timestamp)
                                              .ToListAsync();
            return View(weightEntries);
        }


        public async Task<IActionResult> Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WeightEntry weightEntry)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                weightEntry.Id = userId;
                weightEntry.Timestamp = DateTime.Now;

                _context.Add(weightEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirm", weightEntry);
            }
            return View();
        }


        public async Task<IActionResult> Confirm(WeightEntry weightEntry)
        {
            return View(weightEntry);
        }

    }
}
