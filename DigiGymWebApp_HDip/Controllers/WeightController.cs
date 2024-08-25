using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var userId = _userManager.GetUserId(User);
            var weightEntries = await _context.WeightEntries
                                              .OrderBy(w => w.Timestamp)
                                              .Where(we => we.Id == userId)
                                              .ToListAsync();
            return View(weightEntries);
        }

        public async Task<IActionResult> Create()
        {
            return View("Create");
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

        public async Task<IActionResult> Chart()
        {
            var userId = _userManager.GetUserId(User);
            var allWeight = await _context.WeightEntries
                                .OrderBy(w => w.Timestamp)
                                .Where(x => x.Id == userId)
                                .ToListAsync();
            return View(allWeight);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var weightEntry = await _context.WeightEntries
                                  .Where(w => w.WeightID == id && w.Id == userId)
                                  .FirstOrDefaultAsync();
            if (weightEntry == null)
            {
                return NotFound();
            }
            return View(weightEntry);
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var weightEntry = await _context.WeightEntries
                                          .Where(f => f.WeightID == id && f.Id == userId)
                                          .FirstOrDefaultAsync();
            _context.Remove(weightEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("WeightEntries");
        }

        public async Task<IActionResult> Edit(int? id)
        {   
            var userId = _userManager.GetUserId(User);
            var weightEntry = await _context.WeightEntries
                                  .Where(f => f.WeightID == id && f.Id == userId)
                                   // Return first match
                                  .FirstOrDefaultAsync();
            return View(weightEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WeightID,Weight,Timestamp")] WeightEntry weightEntry)
        {
            if (ModelState.IsValid)
            { 
                var userId = _userManager.GetUserId(User);
                var existingWeightEntry = await _context.WeightEntries
                                                  .Where(f => f.WeightID == id && f.Id == userId)
                                                   // Important
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

                weightEntry.Id = existingWeightEntry.Id;
                weightEntry.Timestamp = existingWeightEntry.Timestamp;
                //weightEntry.Timestamp = DateTime.Now // don't use because then the date changes, what if you edit a few days later? no.

                _context.Update(weightEntry);
                await _context.SaveChangesAsync();

                // redirect to details page of item after editing item
                return RedirectToAction("Details", new { id = weightEntry.WeightID });
            }
            return View();
        }
    }
}
