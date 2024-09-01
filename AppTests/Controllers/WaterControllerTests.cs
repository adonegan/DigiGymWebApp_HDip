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
    public class WaterControllerTests
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
            _context.WaterEntries.AddRange(
                new Water
                {
                    WaterID = 1,
                    Amount = 250,
                    Timestamp = new DateTime(2024, 8, 27),
                    Id = "testuser123"
                },
                new Water
                {
                    WaterID = 2,
                    Amount = 500,
                    Timestamp = new DateTime(2024, 8, 26),
                    Id = "testuser123"
                },
                new Water
                {
                    WaterID = 3,
                    Amount = 750,
                    Timestamp = new DateTime(2024, 8, 25),
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
            var controller = new WaterController(_context, _userManagerMock.Object);

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
            // test simulates a post request to create a new water entry
            // also tests the redirect action goes to Confirm page and that the new water entry saves to the db 
            // arrange
            var userId = "testuser123";
            var water = new Water
            {
                WaterID = 4,
                Amount = 1000,
                Timestamp = new DateTime(2024, 8, 13),
                Id = "testuser123"
            };

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Create(water);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Confirm", redirectResult.ActionName, "Redirect action name should be 'Confirm'");

            // act + assert
            var savedWater = await _context.WaterEntries.FirstOrDefaultAsync(w => w.WaterID == water.WaterID);
            Assert.IsNotNull(savedWater, "Water entry is saved to the database");
            Assert.AreEqual(water.WaterID, savedWater.WaterID, "Saved water id matches water id");
        }

        [TestMethod]
        public async Task Confirm_View()
        {
            // tests confirm action and water entry
            // checks record is not null, is ViewResult and model properties match
            // arrange
            var controller = new WaterController(_context, _userManagerMock.Object);
            var water = await _context.WaterEntries.FirstOrDefaultAsync(w => w.WaterID == 3);

            // act + assert
            var result = await controller.Confirm(water);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result; // cast as ViewResult
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOfType(viewResult.Model, typeof(Water), "Model should be of type water entry");

            // act + assert
            var model = (Water)viewResult.Model;
            Assert.AreEqual(water.WaterID, model.WaterID, "WaterID should match");
            Assert.AreEqual(water.Amount, model.Amount, "Amount should match");
            Assert.AreEqual(water.Timestamp, model.Timestamp, "Timestamp should match");
            Assert.AreEqual(water.Id, model.Id, "Id should match");
        }

        [TestMethod]
        public async Task Delete_Action()
        {
            // tests the deletion of an existing record in test db
            // checks record is not null initially, checks redirection works, checks if deleted record remains in db
            // arrange
            var userId = "testuser123";
            var waterIdToDelete = 200; 

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            _context.WaterEntries.Add(new Water
            {
                WaterID = waterIdToDelete,
                Amount = 2000,
                Timestamp = new DateTime(2024, 8, 20),
                Id = "testuser123"
            });
            _context.SaveChanges();

            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Delete(waterIdToDelete);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("WaterEntries", redirectResult.ActionName, "Redirect action name should be 'WaterEntries'");

            var waterEntry = await _context.WaterEntries
                                    .Where(wa => wa.WaterID == waterIdToDelete && wa.Id == userId)
                                    .FirstOrDefaultAsync();
            Assert.IsNull(waterEntry, "Water entry should not be in the database");
        }


        [TestMethod]
        public async Task WaterEntries_View()
        {
            // tests all records in test db, which are seeded at intialisation + viewResult is returned
            // arrange
            var userId = "testuser123";
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.WaterEntries();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");

            // act + assert
            var model = (List<Water>)viewResult.Model;

            Assert.IsNotNull(model, "Expected List<Water> model type");
            Assert.AreEqual(3, model.Count, "Model should contain 3 water entries for the user.");
            Assert.AreEqual(new DateTime(2024, 8, 25), model[0].Timestamp, "1st water entry should have correct date.");
            Assert.AreEqual(new DateTime(2024, 8, 26), model[1].Timestamp, "2nd water entry should have correct date.");
            Assert.AreEqual(new DateTime(2024, 8, 27), model[2].Timestamp, "3rd water should have correct date.");
        }


        [TestMethod]
        public async Task Details_1()
        {
            // tests that valid Id returns viewResult with correct model properties
            // arrange
            var userId = "testuser123"; 
            var waterId = 2; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Details(waterId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            var model = (Water)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(waterId, model.WaterID);        
        }

        [TestMethod]
        public async Task Details_2()
        {
            // tests that invalid water id returns Not Found
            // arrange
            var userId = "testuser123"; 
            var invalidWaterId = 103; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Details(invalidWaterId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "Result should be a NotFoundResult");
        }

        [TestMethod]
        public async Task Details_3()
        {
            // tests that if there's an unauthorised user, result is Not Found
            // arrange
            var validWaterId = 1;
            var unauthorizedUserId = "unauthorizedUser"; 

            // mock GetUserId to return a user ID that does not match the one used for the water entry
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(unauthorizedUserId);
            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Details(validWaterId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "Result should be a NotFoundResult");
        }

        [TestMethod]
        public async Task Edit_1()
        {
            // tests that valid id returns viewResult with correct model
            // arrange
            var userId = "testuser123"; 
            var waterId = 1; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WaterController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Edit(waterId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            // assert that the model is of type Water and has the correct values
            var model = (Water)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(waterId, model.WaterID, "WaterID should match");
        }

        [TestMethod]
        public async Task Edit_2()
        {
            // tests redirection to Details page, that a record gets updated and verifies the new water entry is in db
            // for this I'm creating a new record first instead of using a seeded one
            // arrange
            var userId = "testuser123";
            var waterId = 5; // new example WaterID for testing, make sure this ID matches the data to be added to db below

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newWaterEntry = new Water
            {
                WaterID = waterId,
                Amount = 650,
                Timestamp = new DateTime(2024, 8, 16),
                Id = "testuser123"
            };

            // add the new entry to db
            _context.WaterEntries.Add(newWaterEntry);
            _context.SaveChanges();

            var updateToNewWaterEntry = new Water
            {
                WaterID = waterId,
                Amount = 2500, // updated
                Timestamp = new DateTime(2024, 8, 16),
                Id = "testuser123"
            };

            var controller = new WaterController(_context, _userManagerMock.Object);

            // Detach any existing tracked entity with the same key
            // System.InvalidOperationException: The instance of entity type 'Water' cannot be tracked because another instance with the, AsNoTracking() issue
            var existingEntry = _context.ChangeTracker.Entries<Water>()
                                         .FirstOrDefault(e => e.Entity.WaterID == waterId);
            if (existingEntry != null)
            {
                _context.Entry(existingEntry.Entity).State = EntityState.Detached;
            }

            // act + assert
            var result = await controller.Edit(waterId, updateToNewWaterEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Details", redirectResult.ActionName, "Redirect action name should be 'Details'");

            // check that the water entry was updated in the database
            var updatedEntry = await _context.WaterEntries
                                .Where(w => w.WaterID == waterId && w.Id == userId)
                                .FirstOrDefaultAsync();
            Assert.IsNotNull(updatedEntry, "Updated water entry should not be null");
            Assert.AreEqual(updateToNewWaterEntry.Amount, updatedEntry.Amount, "Amount should match");
        }

        [TestMethod]
        public async Task Edit_3()
        {
            // tests that an invalid model state returns viewResult and does not update db
            // first I'm creating a new water entry and not using the seeded entries above
            // arrange
            var userId = "testuser123";
            var waterId = 7; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newWaterEntry = new Water
            {
                WaterID = waterId,
                Amount = 780,
                Timestamp = new DateTime(2024, 8, 18),
                Id = "testuser123"
            };


            // add the new entry to db
            _context.WaterEntries.Add(newWaterEntry);
            _context.SaveChanges();

            var controller = new WaterController(_context, _userManagerMock.Object);

            // add invalid model state
            controller.ModelState.AddModelError("Amount", "Required");

            // create invalid entry, Amount is missing
            var invalidWaterEntry = new Water
            {
                WaterID = waterId,
                Timestamp = new DateTime(2024, 8, 18),
                Id = "testuser123"
            };

            // get current state of waterId 7 in db BEFORE calling controller.Edit()
            Water originalWaterEntry;
            originalWaterEntry = _context.WaterEntries.Find(waterId);

            // act + assert
            var result = await controller.Edit(waterId, invalidWaterEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsTrue(viewResult.ViewData.ModelState.ContainsKey("Amount"), "ModelState should contain 'Amount' error"); // isTrue because db did not change as model passed in was invalid
            Assert.AreEqual("Required", viewResult.ViewData.ModelState["Amount"].Errors[0].ErrorMessage, "ModelState error message should be 'Required'");


            // get pre-controller.Edit() water entry state and compare to current db state to verify that water entry was not updated
            var currentWaterEntry = _context.WaterEntries.Find(waterId);
            Assert.IsNotNull(currentWaterEntry, "Water entry should still exist in the database");

            // Assert that the database values have not changed
            Assert.AreEqual(originalWaterEntry.Amount, currentWaterEntry.Amount, "Amount should not have been updated");
        }
    }
}