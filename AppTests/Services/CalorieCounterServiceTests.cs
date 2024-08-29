using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppTests.Services
{
    [TestClass]
    public class CalorieCounterServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private CalorieCounterService _calorieCounterService;

        // setup
        [TestInitialize]    
        public void Setup()
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
            _context.FoodDiary.AddRange(
                new Food
                {
                    FoodID = 1,
                    FoodName = "Breadroll",
                    FoodBrand = "Cuisine De France",
                    Serving = 1,
                    Calories = 160,
                    Protein = 5,
                    Carbohydrates = 60,
                    Fat = 20,
                    MealType = MealTypes.Lunch,
                    Grams = 60,
                    CreatedAt = new DateTime(2024,8,27),
                    Id = "testuser123"
                },
                new Food
                {
                    FoodID = 2,
                    FoodName = "Banana",
                    FoodBrand = "Chiquita",
                    Serving = 1,
                    Calories = 41,
                    Protein = 8,
                    Carbohydrates = 70,
                    Fat = 30,
                    MealType = MealTypes.Lunch,
                    Grams = 50,
                    CreatedAt = new DateTime(2024,8,27),
                    Id = "testuser123"
                },
                new Food
                {
                    FoodID = 3,
                    FoodName = "Nutrigrain Bar",
                    FoodBrand = "Kellogs",
                    Serving = 1,
                    Calories = 174,
                    Protein = 20,
                    Carbohydrates = 90,
                    Fat = 50,
                    MealType = MealTypes.Snack,
                    Grams = 45,
                    CreatedAt = new DateTime(2024,8,28),
                    Id = "testuser123"
                }
            );
            _context.SaveChanges();
            _calorieCounterService = new CalorieCounterService(_context); // important, initialize!
        }

        [TestMethod]
        public async Task Count_OneRecord()
        {
            // arrange
            var date = new DateTime(2024,8,28);
            var userId = "testuser123";

            // act
            var result = await _calorieCounterService.GetTotalCalories(date, userId);

            // assert
            Assert.AreEqual(174, result, "Should be 174.");
        }

        [TestMethod]
        public async Task Count_MultipleRecords()
        {
            // arrange
            var date = new DateTime(2024,8,27);
            var userId = "testuser123";

            // act
            var result = await _calorieCounterService.GetTotalCalories(date, userId);

            // assert
            Assert.AreEqual(201, result, "Should be 201 (sum of calories of the two records).");
        }
    }
}