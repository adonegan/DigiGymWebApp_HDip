using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _context.Database.EnsureCreated();
        }
        
        public async Task<ActionResult> Workouts()
        {
            var userId = _userManager.GetUserId(User);
            var workouts = await _context.Workouts
                .Where(f => f.Id == userId)
                .Select(f => f.Date.Date)
                 // Show single date, even if date has multiple entries
                .Distinct()
                .ToListAsync();
            return View(workouts);
        }

        public async Task<ActionResult> Dates(DateTime date)
        {
            var userId = _userManager.GetUserId(User);
            var workoutEntry = await _context.Workouts
                    .Where(f => f.Date.Date == date.Date && f.Id == userId)
            .ToListAsync();

            return View(workoutEntry);
        }

        public async Task<IActionResult> Create()
        {
            // Workout Type dropdown
            var enumWorkoutTypeValues = Enum.GetValues(typeof(WorkoutTypes));
            var selectListWorkoutType = new SelectList(enumWorkoutTypeValues);
            ViewBag.WorkoutType = selectListWorkoutType;

            // Effort Level dropdown
            var enumEffortLevelValues = Enum.GetValues(typeof(EffortLevels));
            var selectListEffortLevel = new SelectList(enumEffortLevelValues);
            ViewBag.EffortLevel = selectListEffortLevel;

            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workout workout)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                workout.Id = userId;

                _context.Add(workout);
                await _context.SaveChangesAsync();

                return RedirectToAction("Confirm", workout);
            }
            return View();
        }

        public async Task<IActionResult> Confirm(Workout workout)
        {
            return View(workout);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var workoutEntry = await _context.Workouts
                                  .Where(f => f.WorkoutID == id && f.Id == userId)
                                  .FirstOrDefaultAsync();

            if (workoutEntry == null)
            {
                return NotFound();
            }

            return View(workoutEntry);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var workoutEntry = await _context.Workouts
                                          .Where(f => f.WorkoutID == id && f.Id == userId)
                                          .FirstOrDefaultAsync();

            _context.Remove(workoutEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("Workouts");
        }

        public async Task<IActionResult> Edit(int? id)
        {   
            var userId = _userManager.GetUserId(User);
            var workoutEntry = await _context.Workouts
                                  .Where(f => f.WorkoutID == id && f.Id == userId)
                                   // Return first match
                                  .FirstOrDefaultAsync();

            // Workout Type dropdown
            var enumWorkoutTypeValues = Enum.GetValues(typeof(WorkoutTypes));
            var selectListWorkoutType = new SelectList(enumWorkoutTypeValues, workoutEntry.WorkoutType);
            ViewBag.WorkoutType = selectListWorkoutType;

            // Effort Level dropdown
            var enumEffortLevelValues = Enum.GetValues(typeof(EffortLevels));
            var selectListEffortLevel = new SelectList(enumEffortLevelValues, workoutEntry.EffortLevel);
            ViewBag.EffortLevel = selectListEffortLevel;

            return View(workoutEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkoutID,WorkoutType,StartTime,EndTime,EffortLevel,OtherInfo,Date")] Workout workoutEntry)
        {
            if (ModelState.IsValid)
            { 
                var userId = _userManager.GetUserId(User);
                var existingWorkoutEntry = await _context.Workouts
                                                  .Where(f => f.WorkoutID == id && f.Id == userId)
                                                   // Important
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

                workoutEntry.Id = existingWorkoutEntry.Id;
                workoutEntry.Date = existingWorkoutEntry.Date;

                _context.Update(workoutEntry);
                await _context.SaveChangesAsync();

                // redirect to details page of item after editing item
                return RedirectToAction("Details", new { id = workoutEntry.WorkoutID });
            }
            return View();
        }
    }
}
