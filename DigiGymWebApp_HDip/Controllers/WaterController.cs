using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "ClientOnly")]
    public class WaterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WaterController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _context.Database.EnsureCreated();
        }

        public async Task<IActionResult> WaterEntries()
        {
            var userId = _userManager.GetUserId(User);
            var waterEntries = await _context.WaterEntries
                                              .OrderBy(w => w.Timestamp)
                                              .Where(we => we.Id == userId)
                                              .ToListAsync();
            return View(waterEntries);
        }

        public async Task<IActionResult> Create()
        {
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Water waterEntry)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                waterEntry.Id = userId;
                waterEntry.Timestamp = DateTime.Now;

                _context.Add(waterEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirm", waterEntry);
            }
            return View();
        }

        public async Task<IActionResult> Confirm(Water waterEntry)
        {
            return View(waterEntry);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var waterEntry = await _context.WaterEntries
                                  .Where(w => w.WaterID == id && w.Id == userId)
                                  .FirstOrDefaultAsync();
            if (waterEntry == null)
            {
                return NotFound();
            }
            return View(waterEntry);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var waterEntry = await _context.WaterEntries
                                          .Where(f => f.WaterID == id && f.Id == userId)
                                          .FirstOrDefaultAsync();
            _context.Remove(waterEntry);
            await _context.SaveChangesAsync();
            return RedirectToAction("WaterEntries");
        }

        public async Task<IActionResult> Edit(int? id)
        {   
            var userId = _userManager.GetUserId(User);
            var waterEntry = await _context.WaterEntries
                                  .Where(f => f.WaterID == id && f.Id == userId)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync();

            return View(waterEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WaterID,Amount,Timestamp")] Water waterEntry)
        {
            if (ModelState.IsValid)
            { 
                var userId = _userManager.GetUserId(User);
                var existingWaterEntry = await _context.WaterEntries
                                                  .Where(f => f.WaterID == id && f.Id == userId)
                                                  .FirstOrDefaultAsync();

                existingWaterEntry.Amount = waterEntry.Amount;

                _context.Update(existingWaterEntry);
                await _context.SaveChangesAsync();

                // redirect to details page of item after editing item
                return RedirectToAction("Details", new { id = waterEntry.WaterID });
            }
            return View();
        }

        public async Task<IActionResult> Chart()
        {
            var userId = _userManager.GetUserId(User);

            // Get today's date range
            var today = DateTime.Today;
            var startOfDay = today;
            var endOfDay = today.AddDays(1).AddTicks(-1); // End of today, last moment e.g. 23:59:59.9999999

            // Fetch water entries for today
            var dailyWaterEntries = await _context.WaterEntries
                .Where(x => x.Id == userId && x.Timestamp >= startOfDay && x.Timestamp <= endOfDay)
                .ToListAsync();

            // Calculate total intake for today
            var totalIntakeToday = dailyWaterEntries.Sum(w => w.Amount);

            return View(totalIntakeToday);

        }
    }
}
