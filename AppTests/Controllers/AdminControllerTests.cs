using DigiGymWebApp_HDip.Controllers;
using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTests.Controllers
{
    [TestClass]
    public class AdminControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<RoleManager<IdentityRole>>_roleManagerMock;

        [TestInitialize]
        public void Initialize()
        {
            // setup
            var uniqueDatabaseName = Guid.NewGuid().ToString();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: uniqueDatabaseName)
                .Options;

            _context = new ApplicationDbContext(_options);

            // seed user data
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "testuser1@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    UserType = UserTypes.Admin,
                    Id = "testuser123"
                },
                new ApplicationUser
                {
                    UserName = "testuser2@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    UserType = UserTypes.Client,
                    Id = "testuser124"
                }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // mock FindByIdAsync method
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                        .ReturnsAsync((string id) => _context.Users.FirstOrDefault(u => u.Id == id));

            // returns queryable list of users
            _userManagerMock.Setup(um => um.Users).Returns(_context.Users.AsQueryable());

            // set up mock roles
            var roles = new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("Client"),
                new IdentityRole("Trainer")
            }.AsQueryable();

            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                // RoleManager dependencies
                new Mock<IRoleStore<IdentityRole>>().Object,
                new IRoleValidator<IdentityRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

            _roleManagerMock.Setup(r => r.Roles).Returns(roles);
        }
    
        [TestMethod]
        public async Task Admin_View()
        {
            // tests view is not null and result returned is viewResult
            // arrange
            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.Admins();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");
        }

        [TestMethod]
        public async Task Trainers_View()
        {
            // tests view is not null and result returned is viewResult
            // arrange
            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.Trainers();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");
        }

        [TestMethod]
        public async Task Client_View()
        {
            // tests view is not null and result returned is viewResult
            // arrange
            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.Clients();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");
        }

        [TestMethod]
        public async Task Users_View()
        {
            // tests view is not null and result returned is viewResult
            // arrange
            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.Users();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");
        }

        [TestMethod]
        public async Task PendingApproval_View()
        {
            // tests view is not null and result returned is viewResult
            // arrange
            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.PendingApproval();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");
        }

        [TestMethod]
        public async Task ManageUser_View()
        {
            // tests view is not null and result returned is viewResult
            // arrange
            var userToManageId = "testuser123";
            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.ManageUser(userToManageId);
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");
        }

        [TestMethod]
        public async Task DowngradeAdmin_Action()
        {
            // tests that an Admin user is downgraded to Trainer, redirection to ManageUser view and view is not null
            // arrange
            var AdminIDToDowngrade = "testuser130"; 
            var newUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                UserType = UserTypes.Admin,
                Id = AdminIDToDowngrade
            };

            // Add the new entry to the _context
            _context.Users.Add(newUser);
            _context.SaveChanges();

            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.DowngradeAdmin(AdminIDToDowngrade);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("ManageUser", redirectResult.ActionName, "Redirect action name should be 'ManageUser'");
             
            // verify the user has been downgraded
            var downgradedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == AdminIDToDowngrade);
            Assert.IsNotNull(downgradedUser, "Record should not be null");
            Assert.AreEqual(UserTypes.Trainer, downgradedUser.UserType, "UserType should be Trainer");
        }

        [TestMethod]
        public async Task UpgradeToAdmin_Action()
        {
            // tests that a Trainer user is upgraded to Admin, redirection to ManageUser view and view is not null
            // arrange
            var UserIDToUpgrade = "testuser225"; 
            var newUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                UserType = UserTypes.Trainer,
                Id = UserIDToUpgrade
            };

            // add the new entry to db
            _context.Users.Add(newUser);
            _context.SaveChanges();

            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.UpgradeToAdmin(UserIDToUpgrade);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("ManageUser", redirectResult.ActionName, "Redirect action name should be 'ManageUser'");

            // verify Trainer user has been upgraded
            var upgradedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserIDToUpgrade);
            Assert.IsNotNull(upgradedUser, "Entry should not be null");
            Assert.AreEqual(UserTypes.Admin, upgradedUser.UserType, "UserType should be Admin");
        }

        [TestMethod]
        public async Task ApproveTrainer_Action()
        {
            // tests that a Trainer user has an approval status of Approved, redirection to ManaageUser view and view is not null
            // for this I'm creating a new user record
            // arrange
            var TrainerToApprove = "testuser335"; 
            var newUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                UserType = UserTypes.Trainer,
                ApprovalStatus = ApprovalStatuses.None, // no approval yet
                Id = TrainerToApprove
            };

            // add the new entry to db
            _context.Users.Add(newUser);
            _context.SaveChanges();

            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.ApproveTrainer(TrainerToApprove);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("ManageUser", redirectResult.ActionName, "Redirect action name should be 'ManagerUser'");

            // verify that the user has new approval status
            var approvedTrainer = await _context.Users.FirstOrDefaultAsync(u => u.Id == TrainerToApprove);
            Assert.IsNotNull(approvedTrainer, "Entry should not be null");
            Assert.AreEqual(UserTypes.Trainer, approvedTrainer.UserType, "UserType should be Trainer");
            Assert.AreEqual(ApprovalStatuses.Approved, approvedTrainer.ApprovalStatus, "Approval status should be 'Approved'");
        }

        [TestMethod]
        public async Task RejectTrainer_Action()
        {
            // tests that a Trainer user has an approval status of Rejected, redirection to ManageUser view and view is not null
            // for this I'm creating a new user record
            // arrange
            var TrainerToReject = "testuser445"; 
            var newUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                UserType = UserTypes.Trainer,
                ApprovalStatus = ApprovalStatuses.None, // no approval yet
                Id = TrainerToReject
            };

            // add the new entry to db
            _context.Users.Add(newUser);
            _context.SaveChanges();

            var controller = new AdminController(_context, _userManagerMock.Object, _roleManagerMock.Object);

            // act + assert
            var result = await controller.RejectTrainer(TrainerToReject);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("ManageUser", redirectResult.ActionName, "Redirect action name should be 'ManagerUser'");

            // verify the user's approval is rejected
            var rejectedTrainer = await _context.Users.FirstOrDefaultAsync(u => u.Id == TrainerToReject);
            Assert.IsNotNull(rejectedTrainer, "Entry should not be null");
            Assert.AreEqual(UserTypes.Trainer, rejectedTrainer.UserType, "UserType should be Trainer");
            Assert.AreEqual(ApprovalStatuses.Rejected, rejectedTrainer.ApprovalStatus, "Approval status should be 'Rejected'");
        }
    }
}