using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using DigiGymWebApp_HDip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AppTests.Services
{
    [TestClass]
    public class BMIServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private BMIService _bmiService;

        // setup
        [TestInitialize]    
        public void Setup()
        {
            // setup
            var uniqueDatabaseName = Guid.NewGuid().ToString();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: uniqueDatabaseName)
                .Options;

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // seed user and test data
            using (var context = new ApplicationDbContext(_options))
            {
                var testUser = new ApplicationUser
                {
                    UserName = "testuser@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Id = "testuser123" 
                };
                context.Users.Add(testUser);
                context.ProfileEntries.Add(
                    new UserProfile
                    {
                        ProfileID = 1,
                        Height = 161,
                        Gender = GenderTypes.Female,
                        Id = "testuser123"
                    }
                );
                context.WeightEntries.Add(
                    new WeightEntry
                    {
                        WeightID = 1,
                        Weight = 150,
                        Timestamp = new DateTime(2024,8,27),
                        Id = "testuser123"
                    }
                );
                context.SaveChanges();

            }
            _context = new ApplicationDbContext(_options);
            _bmiService = new BMIService(_context); // important, initialize!
        }


        [TestMethod]
        public async Task Get_UserBMI()
        {
            // arrange
            var date = new DateTime(2024,8,27);
            var userId = "testuser123";

            // act
            var result = await _bmiService.GetBMI(date, userId);

            // assert
            Assert.AreEqual(26.2, result, "Should be 26.2, which is overweight");
        }
    }
}