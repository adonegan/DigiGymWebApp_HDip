using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Helpers;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DigiGymWebApp_HDip.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>_roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _context.Database.EnsureCreated();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> PendingApproval()
        {
            var pendingUsers = await _userManager.Users
                                  .Where(p => p.UserType == UserTypes.Trainer )
                                  .ToListAsync();
            return View(pendingUsers);
        }

        public async Task<IActionResult> ManageUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return View(user);
        }

        public async Task<IActionResult> ApproveTrainer(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            user.ApprovalStatus = ApprovalStatuses.Approved;

            _context.SaveChanges();
            return RedirectToAction("ManageUser", user);
        }

        public async Task<IActionResult> RejectTrainer(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            user.ApprovalStatus = ApprovalStatuses.Rejected;

            _context.SaveChanges();
            return RedirectToAction("ManageUser", user);
        }

        public async Task<IActionResult> Trainers()
        {
            var trainers = await _userManager.Users
                                .Where(u=>u.UserType == UserTypes.Trainer && u.ApprovalStatus == ApprovalStatuses.Approved)
                                .ToListAsync();
            return View(trainers);
        }

        public async Task<IActionResult> Admins()
        {
            var admins = await _userManager.Users
                                .Where(a => a.UserType == UserTypes.Admin)
                                .ToListAsync();
            return View(admins);
        }

        public async Task<IActionResult> UpgradeToAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }
            user.UserType = UserTypes.Admin;
            user.ApprovalStatus = ApprovalStatuses.None;
            _context.SaveChanges();

            return RedirectToAction("ManageUser", user);
        }

        // Admins are downgraded to Trainers
        // Would be a different role if more employee roles are added later!
        public async Task<IActionResult> DowngradeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }
            user.UserType = UserTypes.Trainer;
            user.ApprovalStatus = ApprovalStatuses.None;
            _context.SaveChanges();

            return RedirectToAction("ManageUser", user);
        }

        public async Task<IActionResult> Clients()
        {
            var clients = await _userManager.Users
                .Where(c=>c.UserType == UserTypes.Client)
                .ToListAsync();

            return View(clients);
        }
    }
}
