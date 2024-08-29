using DigiGymWebApp_HDip.Controllers;
using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace AppTests.Controllers
{
   [TestClass]
    public class FoodControllerTest
    {
        // declare private fields for setup / initalization in tests
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<ICalorieCounterService> _mockCalorieCounterService;

        [TestInitialize]
        public void Initialize()
        {
            // create unique db for multiple testcases
            var uniqueDatabaseName = Guid.NewGuid().ToString();

            // initialize in-memory database
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: uniqueDatabaseName)
                .Options;

            // set up shared context for all test methods
            _context = new ApplicationDbContext(_options);

            // set up usermanager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

             // create mock object of the calorie counter service
            _mockCalorieCounterService = new Mock<ICalorieCounterService>();

            // seed data - 3 records + test user data
            var testUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Id = "testuser123" 
            };
            _context.Users.Add(testUser);
            _context.FoodDiary.AddRange(
                new Food
                {
                    FoodID = 1,
                    FoodName = "Bread",
                    FoodBrand = "Brennans",
                    Serving = 2,
                    Calories = 174,
                    Protein = 5,
                    Carbohydrates = 16,
                    Fat = 20,
                    MealType = MealTypes.Breakfast,
                    Grams = 100,
                    CreatedAt = new DateTime(2024, 8, 10),
                    Id = "testuser123"
                },
                new Food
                {
                    FoodID = 2,
                    FoodName = "Steak",
                    FoodBrand = "Dunnes Stores",
                    Serving = 2,
                    Calories = 575,
                    Protein = 25,
                    Carbohydrates = 10,
                    Fat = 30,
                    MealType = MealTypes.Dinner,
                    Grams = 250,
                    CreatedAt = new DateTime(2024, 8, 9),
                    Id = "testuser123"
                },
                new Food
                {
                    FoodID = 3,
                    FoodName = "Nutrigrain Bar",
                    FoodBrand = "Kellogg's",
                    Serving = 1,
                    Calories = 175,
                    Protein = 15,
                    Carbohydrates = 20,
                    Fat = 25,
                    MealType = MealTypes.Snack,
                    Grams = 44,
                    CreatedAt = new DateTime(2024, 8, 7),
                    Id = "testuser123"
                }
            );
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task Dates_View()
        {
            // tests that controllers returns viewResult and that correct model properties are returned
            // arrange
            var userId = "testuser123"; 
            var testDate = new DateTime(2024, 8, 9); 
            var expectedCalories = 575; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // mock the calorie counter service
            _mockCalorieCounterService.Setup(s => s.GetTotalCalories(testDate, userId)).ReturnsAsync(expectedCalories);

            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Dates(testDate);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "The result should be a ViewResult.");

            // act + assert
            var model = viewResult.Model as List<Food>;
            Assert.IsNotNull(model, "The model should be of type List<Food>.");
            // check that the correct food entries were retrieved
            Assert.AreEqual(1, model.Count, "Model should contain 1 food entry for the specified date.");
            Assert.AreEqual("Steak", model.First().FoodName, "The retrieved food entry should be 'Steak'.");

            // act + assert
            var viewData = viewResult.ViewData;
            Assert.AreEqual(expectedCalories, viewData["TotalCalories"], "TotalCalories should match the mocked value.");
            Assert.AreEqual(25, viewData["TotalProtein"], "TotalProtein should match the sum of the retrieved entries.");
            Assert.AreEqual(10, viewData["TotalCarbs"], "TotalCarbs should match the sum of the retrieved entries.");
            Assert.AreEqual(30, viewData["TotalFat"], "TotalFat should match the sum of the retrieved entries.");
        }

        [TestMethod]
        public async Task FoodDiary_1()
        {
            // tests that viewResult is returned and only with distinct dates
            // arrange
            var userId = "testuser123";
            
            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, null);

            // act + assert
            var result = await controller.FoodDiary(userId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");
            
            // act + assert
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as List<DateTime>;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(3, model.Count, "Model should contain 3 distinct dates");
            Assert.IsTrue(model.Contains(new DateTime(2024, 8, 9)), "Model should contain August 9, 2024");
            Assert.IsTrue(model.Contains(new DateTime(2024, 8, 10)), "Model should contain August 10, 2024");
        }

        [TestMethod]
        public async Task FoodDiary_2()
        {
            // tests that if there are no entries for a user an empty list is returned
            // arrange
            var userId = "nonexistentuser";
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, null);

            // act
            var result = await controller.FoodDiary(userId);
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as List<DateTime>;

            // assert
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(0, model.Count, "Model should be an empty list when no entries exist");
        }

        [TestMethod]
        public async Task FoodDiary_3()
        {
            // tests that entries for other users are not included in current user's food diary
            // arrange
            var userId = "testuser123";
            var otherUserId = "otheruser123";

            _context.FoodDiary.Add(new Food
            {
                FoodID = 6,
                FoodName = "Orange",
                FoodBrand = "Kellogg's",
                Serving = 1,
                Calories = 175,
                Protein = 15,
                Carbohydrates = 20,
                Fat = 25,
                MealType = MealTypes.Snack,
                Grams = 44,
                CreatedAt = new DateTime(2024, 8, 20),
                Id = otherUserId
            });
            _context.SaveChanges();

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, null);

            // act
            var result = await controller.FoodDiary(userId);
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as List<DateTime>;

            // assert
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(3, model.Count, "Model should still contain only 3 distinct dates");
            Assert.IsFalse(model.Contains(new DateTime(2024, 8, 20)), "Model should not contain dates from other users");
        }

        [TestMethod]
        public async Task Create_View()
        {
            // tests that ViewResult is returned
            // arrange
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Create();
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.AreEqual("Create", viewResult.ViewName, "The view name should be 'Create'");
            // verifies key "MealType" exists in ViewData dictionary, confirming that data was correctly added to ViewData.
            Assert.IsTrue(viewResult.ViewData.ContainsKey("MealType"), "ViewData should contain 'MealType'");

            // act + assert
            var selectList = (SelectList)viewResult.ViewData["MealType"];
            Assert.IsNotNull(selectList, "ViewData['MealType'] should be a SelectList");
        }

        [TestMethod]
        public async Task Create_Post()
        {
            // tests that redirection is to Confirm page, new entry added to db
            // arrange
            var userId = "testuser123";
            var foodId = 4;
            var food = new Food
            {
                FoodID = foodId,
                FoodName = "Cappucino",
                FoodBrand = "Nescafe",
                Serving = 1,
                Calories = 60,
                Protein = 2,
                Carbohydrates = 25,
                Fat = 4,
                MealType = MealTypes.Snack,
                Grams = 220,
                CreatedAt = DateTime.Now,
                Id = "testuser123"
            };

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Create(food);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Confirm", redirectResult.ActionName, "Redirect action name should be 'Confirm'");

            // verify that the Food record is added to the database
            var savedFood = await _context.FoodDiary.FirstOrDefaultAsync(f => f.FoodName == food.FoodName);
            Assert.IsNotNull(savedFood, "Food entry should be saved to the database");
            Assert.AreEqual(food.FoodName, savedFood.FoodName, "Saved food name should match");
        }

        [TestMethod]
        public async Task Confirm_View()
        {
            // tests that view returned is not null and that model properties passed are correct
            // arrange
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);
            var food = new Food
            {
                FoodID = 6,
                FoodName = "Tea",
                FoodBrand = "Barry's",
                Serving = 1,
                Calories = 60,
                Protein = 2,
                Carbohydrates = 5,
                Fat = 4,
                MealType = MealTypes.Snack,
                Grams = 5,
                CreatedAt = DateTime.Now,
                Id = "testuser123"
            };

            // act + assert
            var result = await controller.Confirm(food);
            Assert.IsNotNull(result, "It should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "It should be a ViewResult");

            // use cast operator to make IActionResult ViewResult instead
            // do this to access the Model property
            var viewResult = (ViewResult)result;
            // model contains the data the view will render, hence testing
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOfType(viewResult.Model, typeof(Food), "Model should be of type Food");

            // act + assert
            // cast object as instance of Food class
            var model = (Food)viewResult.Model;
            // checks that properties of food object and model passed to the view are the same, this checks that data is passed to the view as expected
            Assert.AreEqual(food.FoodID, model.FoodID, "FoodID should match");
            Assert.AreEqual(food.FoodName, model.FoodName, "FoodName should match");
            Assert.AreEqual(food.FoodBrand, model.FoodBrand, "FoodBrand should match");
            Assert.AreEqual(food.Serving, model.Serving, "Serving should match");
            Assert.AreEqual(food.Calories, model.Calories, "Calories should match");
            Assert.AreEqual(food.Protein, model.Protein, "Protein should match");
            Assert.AreEqual(food.Carbohydrates, model.Carbohydrates, "Carbohydrates should match");
            Assert.AreEqual(food.Fat, model.Fat, "Fat should match");
            Assert.AreEqual(food.MealType, model.MealType, "MealType should match");
            Assert.AreEqual(food.Grams, model.Grams, "Grams should match");
            Assert.AreEqual(food.CreatedAt, model.CreatedAt, "CreatedAt should match");
            Assert.AreEqual(food.Id, model.Id, "Id should match");
        }

        [TestMethod]
        public async Task Details_1()
        {
            // tests that valid Id returns viewResult with correct model properties
            // arrange
            var userId = "testuser123"; 
            var foodId = 1; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Details(foodId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            var model = (Food)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(foodId, model.FoodID);
            Assert.AreEqual("Bread", model.FoodName);
        }

        [TestMethod]
        public async Task Details_2()
        {
            // tests that invalid food id returns Not Found
            // arrange
            var userId = "testuser123"; 
            var invalidFoodId = 143; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Details(invalidFoodId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "Result should be a NotFoundResult");
        }

        [TestMethod]
        public async Task Details_3()
        {
            // tests that if there's an unauthorised user, result is Not Found
            // arrange
            var validFoodId = 1;
            var unauthorizedUserId = "unauthorizedUser"; 

            // mock GetUserId to return a user ID that does not match the one used for the food entry
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(unauthorizedUserId);
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Details(validFoodId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "Result should be a NotFoundResult");
        }

        [TestMethod]
        public async Task Delete_Action()
        {
            // tests that a valid id in this action removes the food entry and redirects user
            // arrange
            var userId = "testuser123";
            var foodIdToDelete = 333; // Use a new record for this test instead of previously seeded data

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // Create a new context and add test data
            _context.FoodDiary.Add(new Food
            {
                FoodID = foodIdToDelete,
                FoodName = "Cheese",
                FoodBrand = "Farmhouse",
                Serving = 3,
                Calories = 50,
                Protein = 20,
                Carbohydrates = 10,
                Fat = 20,
                MealType = MealTypes.Breakfast,
                Grams = 100,
                CreatedAt = DateTime.Now,
                Id = userId
            });
            _context.SaveChanges();

            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Delete(foodIdToDelete);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("FoodDiary", redirectResult.ActionName, "Redirect action name should be 'FoodDiary'");

            // verify that the food entry was removed from the database
            var foodEntry = await _context.FoodDiary
                                    .Where(f => f.FoodID == foodIdToDelete && f.Id == userId)
                                    .FirstOrDefaultAsync();
            Assert.IsNull(foodEntry, "Food entry should be removed from the database");
        }

        [TestMethod]
        public async Task Edit_1()
        {
            // tests that valid id returns viewResult with correct model and selectList 
            // arrange
            var userId = "testuser123"; 
            var foodId = 1; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // act + assert
            var result = await controller.Edit(foodId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            // assert that the model is of type Food and has the correct values
            var model = (Food)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(foodId, model.FoodID, "FoodID should match");

            // act + assert
            // verify ViewBag.MealType exists
            Assert.IsTrue(viewResult.ViewData.ContainsKey("MealType"), "ViewData should contain 'MealType'");
            var selectList = (SelectList)viewResult.ViewData["MealType"];
            Assert.IsNotNull(selectList, "ViewData['MealType'] should be a SelectList");

            // check that the selected value is the same as the model's MealType
            var selectedValue = selectList.SelectedValue?.ToString();
            var expectedValue = model.MealType.ToString();
            Assert.AreEqual(expectedValue, selectedValue, "Selected meal type should match");
        }

        [TestMethod]
        public async Task Edit_2()
        {
            // tests redirection to FoodDiary page, that a record gets updated and verifies the new food entry is in db
            // for this I'm creating a new record first instead of using a seeded one
            // arrange
            var userId = "testuser123";
            var foodId = 4; // new example FoodID for testing, make sure this ID matches the data to be added to db below

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newFoodEntry = new Food
            {
                FoodID = foodId,
                FoodName = "Bread",
                FoodBrand = "Brennans",
                Serving = 2,
                Calories = 174,
                Protein = 5,
                Carbohydrates = 16,
                Fat = 20,
                MealType = MealTypes.Breakfast,
                Grams = 100,
                CreatedAt = DateTime.Now,
                Id = userId
            };

            // add the new entry to db
            _context.FoodDiary.Add(newFoodEntry);
            _context.SaveChanges();

            var updateToNewFoodEntry = new Food
            {
                FoodID = foodId,
                FoodName = "Updated Bread",
                FoodBrand = "Updated Brennans",
                Serving = 3,
                Calories = 200,
                Protein = 6,
                Carbohydrates = 18,
                Fat = 22,
                MealType = MealTypes.Lunch,
                Grams = 150,
                CreatedAt = newFoodEntry.CreatedAt,
                Id = userId
            };

            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // Detach any existing tracked entity with the same key
            // System.InvalidOperationException: The instance of entity type 'Food' cannot be tracked because another instance with the, AsNoTracking() issue
            var existingEntry = _context.ChangeTracker.Entries<Food>()
                                         .FirstOrDefault(e => e.Entity.FoodID == foodId);
            if (existingEntry != null)
            {
                _context.Entry(existingEntry.Entity).State = EntityState.Detached;
            }

            // act + assert
            var result = await controller.Edit(foodId, updateToNewFoodEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("FoodDiary", redirectResult.ActionName, "Redirect action name should be 'FoodDiary'");

            // check that the food entry was updated in the database
            var updatedEntry = await _context.FoodDiary
                                .Where(f => f.FoodID == foodId && f.Id == userId)
                                .FirstOrDefaultAsync();
            Assert.IsNotNull(updatedEntry, "Updated food entry should not be null");
            Assert.AreEqual(updateToNewFoodEntry.FoodName, updatedEntry.FoodName, "FoodName should match");
        }

        [TestMethod]
        public async Task Edit_3()
        {
            // tests that an invalid model state returns viewResult and does not update db
            // first I'm creating a new food entry and not using the seeded entries above
            // arrange
            var userId = "testuser123";
            var foodId = 5; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newFoodEntry = new Food
            {
                FoodID = foodId,
                FoodName = "Bread",
                FoodBrand = "Brennans",
                Serving = 2,
                Calories = 174,
                Protein = 5,
                Carbohydrates = 16,
                Fat = 20,
                MealType = MealTypes.Breakfast,
                Grams = 100,
                CreatedAt = DateTime.Now,
                Id = userId
            };

            // add the new entry to db
            _context.FoodDiary.Add(newFoodEntry);
            _context.SaveChanges();

            var controller = new FoodController(_context, _userManagerMock.Object, _mockCalorieCounterService.Object);

            // add invalid model state
            controller.ModelState.AddModelError("FoodName", "Required");

            // create invalid entry, FoodName is missing
            var invalidFoodEntry = new Food
            {
                FoodID = foodId,
                FoodBrand = "Invalid Brand",
                Serving = 3,
                Calories = 200,
                Protein = 6,
                Carbohydrates = 18,
                Fat = 22,
                MealType = MealTypes.Lunch,
                Grams = 150,
                CreatedAt = newFoodEntry.CreatedAt,
                Id = userId
            };

            // get current state of foodId 5 in db BEFORE calling controller.Edit()
            Food originalFoodEntry;
            originalFoodEntry = _context.FoodDiary.Find(foodId);

            // act + assert
            var result = await controller.Edit(foodId, invalidFoodEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsTrue(viewResult.ViewData.ModelState.ContainsKey("FoodName"), "ModelState should contain 'FoodName' error"); // isTrue because db did not change as model passed in was invalid
            Assert.AreEqual("Required", viewResult.ViewData.ModelState["FoodName"].Errors[0].ErrorMessage, "ModelState error message should be 'Required'");


            // get pre-controller.Edit() food entry state and compare to current db state to verify that food entry was not updated
            var currentFoodEntry = _context.FoodDiary.Find(foodId);
            Assert.IsNotNull(currentFoodEntry, "Food entry should still exist in the database");

            // Assert that the database values have not changed
            Assert.AreEqual(originalFoodEntry.FoodName, currentFoodEntry.FoodName, "FoodName should not have been updated");
            Assert.AreEqual(originalFoodEntry.FoodBrand, currentFoodEntry.FoodBrand, "FoodBrand should not have been updated");
        }
    }
}
