﻿using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Controllers
{
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
                                   // Return first match
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
                                                   // Important
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

                waterEntry.Id = existingWaterEntry.Id;
                waterEntry.Timestamp = existingWaterEntry.Timestamp;

                _context.Update(waterEntry);
                await _context.SaveChangesAsync();

                // redirect to details page of item after editing item
                return RedirectToAction("Details", new { id = waterEntry.WaterID });
            }
            return View();
        }
    }
}