using DigiGymWebApp_HDip.Controllers;
using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace AppTests.Controllers
{
    [TestClass]
    public class WeightControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;

        [TestInitialize]
        public void Initialize()
        {
            // setup
            var uniqueDatabaseName = Guid.NewGuid().ToString();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: uniqueDatabaseName)
                .Options;

            // set up shared db context for all test methods
            _context = new ApplicationDbContext(_options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // seed user and test data
            var testUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Id = "testuser123" 
            };
            _context.Users.Add(testUser);
            _context.WeightEntries.AddRange(
                new WeightEntry
                {
                    WeightID = 1,
                    Weight = 160,
                    Timestamp = new DateTime(2024, 8, 8),
                    Id = "testuser123"
                },
                new WeightEntry
                {
                    WeightID = 2,
                    Weight = 161,
                    Timestamp = new DateTime(2024, 8, 9),
                    Id = "testuser123"
                },
                new WeightEntry
                {
                    WeightID = 3,
                    Weight = 162,
                    Timestamp = new DateTime(2024, 8, 10),
                    Id = "testuser123"
                }
            );
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task Create_View()
        {
            // test is view is not null, is a ViewResult and view name is Create
            // arrange
            var controller = new WeightController(_context, _userManagerMock.Object);

            // act
            var result = await controller.Create();

            // assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            var viewResult = (ViewResult)result;
            Assert.AreEqual("Create", viewResult.ViewName, "The view name should be 'Create'");
        }


        [TestMethod]
        public async Task Create_Post()
        {
            // test simulates a post request to create a new weightentry
            // also tests the redirect action goes to Confirm page and that the new weightentry saves to the db 
            // arrange
            var userId = "testuser123";
            var weight = new WeightEntry
            {
                WeightID = 4,
                Weight = 163,
                Timestamp = new DateTime(2024, 8, 11),
                Id = "testuser123"
            };

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WeightController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Create(weight);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Confirm", redirectResult.ActionName, "Redirect action name should be 'Confirm'");

            // act + assert
            var savedWeight = await _context.WeightEntries.FirstOrDefaultAsync(w => w.WeightID == weight.WeightID);
            Assert.IsNotNull(savedWeight, "Weightentry is saved to the database");
            Assert.AreEqual(weight.WeightID, savedWeight.WeightID, "Saved weight id matches weight id");
        }


        [TestMethod]
        public async Task Confirm_View()
        {
            // tests confirm action and weightentry
            // checks record is not null, is ViewResult and model properties match
            // arrange
            var controller = new WeightController(_context, _userManagerMock.Object);
            var weight = await _context.WeightEntries.FirstOrDefaultAsync(w => w.WeightID == 3);

            // act + assert
            var result = await controller.Confirm(weight);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result; // cast as ViewResult
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOfType(viewResult.Model, typeof(WeightEntry), "Model should be of type weightentry");

            // act + assert
            var model = (WeightEntry)viewResult.Model;
            Assert.AreEqual(weight.WeightID, model.WeightID, "WeightID should match");
            Assert.AreEqual(weight.Weight, model.Weight, "Weight should match");
            Assert.AreEqual(weight.Timestamp, model.Timestamp, "Timestamp should match");
            Assert.AreEqual(weight.Id, model.Id, "Id should match");
        }


        [TestMethod]
        public async Task Delete_Action()
        {
            // tests the deletion of a pre-existing record in test db
            // checks record is not null initially, checks redirection works, checks if deleted record remains in db
            // arrange
            var userId = "testuser123";
            var weightIdToDelete = 100; 

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            _context.WeightEntries.Add(new WeightEntry
            {
                WeightID = weightIdToDelete,
                Weight = 167,
                Timestamp = new DateTime(2024, 8, 17),
                Id = "testuser123"
            });
            _context.SaveChanges();

            var controller = new WeightController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Delete(weightIdToDelete);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("WeightEntries", redirectResult.ActionName, "Redirect action name should be 'WeightEntries'");

            var weightEntry = await _context.WeightEntries
                                    .Where(f => f.WeightID == weightIdToDelete && f.Id == userId)
                                    .FirstOrDefaultAsync();
            Assert.IsNull(weightEntry, "Weight entry should not be in the database");
        }


        [TestMethod]
        public async Task WeightEntries_View()
        {
            // tests all records in test db, which are seeded at intialisation + viewResult is returned
            // arrange
            var userId = "testuser123";
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new WeightController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.WeightEntries();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");

            // act + assert
            var model = (List<WeightEntry>)viewResult.Model;
            Assert.IsNotNull(model, "Expected List<WeightEntry> model type");
            Assert.AreEqual(3, model.Count, "Model should contain 3 weightentries for the user.");
            Assert.AreEqual(160, model[0].Weight, "1st weightentry should have weight of 160.");
            Assert.AreEqual(161, model[1].Weight, "2nd weightentry should have weight of 161.");
            Assert.AreEqual(162, model[2].Weight, "3rd weightentry should have weight of 162.");
            Assert.AreEqual(new DateTime(2024, 8, 8), model[0].Timestamp, "1st weightentry should have correct timestamp.");
            Assert.AreEqual(new DateTime(2024, 8, 9), model[1].Timestamp, "2nd weightentry should have correct timestamp.");
            Assert.AreEqual(new DateTime(2024, 8, 10), model[2].Timestamp, "3rd weightentry should have correct timestamp.");
        }
    }
}