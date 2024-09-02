using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "TrainerOnly")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>_roleManager;

        public TrainerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _context.Database.EnsureCreated();
        }

        public async Task<IActionResult> Requests()
        {
            var userId = _userManager.GetUserId(User);

            // get messages where receiver (trainer) is null
            var messages = await _context.Messages
                .Where(m => m.ReceiverID == null && m.SenderID != userId)
                .Include(m => m.Conversation)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            var trainerRequests = messages
                .GroupBy(m => m.ConversationID)
                .Select(g => g.First())
                .ToList();

            return View(trainerRequests);
        }
    }
}
