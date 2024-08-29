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
    public class ClientControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private Mock<IBMIService> _bmiServiceMock;
        private Mock<IBMICategory> _bmiCategoryMock;
        private ClientController _controller;


        [TestInitialize]
        public void Initialize()
        {
            // setup
            var uniqueDatabaseName = Guid.NewGuid().ToString();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: uniqueDatabaseName)
                .Options;

            // initialize ApplicationDbContext
            // shared db context for all test methods 
            _context = new ApplicationDbContext(_options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // mock RoleManager
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null
            );

            // initialize other mocks
            _bmiServiceMock = new Mock<IBMIService>();
            _bmiCategoryMock = new Mock<IBMICategory>();

            // set up controller instead of repeating constructor in every test case
            _controller = new ClientController(
                _context,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _bmiServiceMock.Object,
                _bmiCategoryMock.Object
            );

            // seed data - 3 records + test user data
            var testUser = new ApplicationUser
            {
                UserName = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Id = "testuser123" 
            };
            _context.Users.Add(testUser);
            _context.ProfileEntries.AddRange(
                new UserProfile
                {
                    ProfileID = 1,
                    Height = 163,
                    Gender = GenderTypes.Male,
                    Id = "testuser123"
                },
                new UserProfile
                {
                    ProfileID = 2,
                    Height = 151,
                    Gender = GenderTypes.Female,
                    Id = "testuser123"
                },
                new UserProfile
                {
                    ProfileID = 3,
                    Height = 150,
                    Gender = GenderTypes.Male,
                    Id = "testuser123"
                }
            );
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task Create_View()
        { 
            // tests that ViewResult is returned, view name is Create and that selectList count is correct
            // act + assert
            var result = await _controller.Create() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Create", result.ViewName, "Name is Create");

            // act + assert
            var selectList = result.ViewData["GenderType"] as SelectList;
            Assert.IsNotNull(selectList);
            // check selectList has same number of items as enum values
            Assert.AreEqual(Enum.GetValues(typeof(GenderTypes)).Cast<GenderTypes>().Count(), selectList.Count());
        }

        [TestMethod]
        public async Task Create_Post()
        {
            // test simulates a post request to create a new UserProfile
            // also tests the redirect action goes to Confirm page and that the new profile saves to the db 
            // arrange
            var userId = "testuser123";
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            
            var profile = new UserProfile
            {
                ProfileID = 5,
                Height = 160,
                Gender = GenderTypes.Female,
                Id = userId
            };

            // act + assert
            var result = await _controller.Create(profile);
            var redirectResult = (RedirectToActionResult)result;
            Assert.IsNotNull(result);
            Assert.AreEqual("Confirm", redirectResult.ActionName, "Action result is Confirm");

            // act + assert
            var savedProfile = await _context.ProfileEntries.FirstOrDefaultAsync(p => p.ProfileID == profile.ProfileID);
            Assert.IsNotNull(savedProfile);
            Assert.AreEqual(profile.Id, savedProfile.Id, "Profile id and savedProfile id are the same");
        }

        [TestMethod]
        public async Task Confirm_View()
        {
            // tests confirm action and profile record
            // checks record is not null, is ViewResult and model properties match
            // arrange
            var userId = "testuser123";
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var profile = new UserProfile
            {
                ProfileID = 6,
                Height = 161,
                Gender = GenderTypes.Male,
                Id = userId
            };

            // act + assert
            var result = await _controller.Confirm(profile);
            Assert.IsNotNull(result, "It should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "It should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOfType(viewResult.Model, typeof(UserProfile), "Model should be of type Food");

            // act + assert
            var model = (UserProfile)viewResult.Model;
            Assert.AreEqual(profile.ProfileID, model.ProfileID, "ProfileID should match");
            Assert.AreEqual(profile.Height, model.Height, "Height should match");
            Assert.AreEqual(profile.Gender, model.Gender, "Gender should match");
            Assert.AreEqual(profile.Id, model.Id, "Id should match");
        }

        [TestMethod]
        public async Task Edit_1()
        {
            // tests that edit action retrieves a profile, that it is a viewResult and model properties match
            // arrange
            var userId = "testuser123"; 
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // existing record (seeded)
            var profileId = 1; 

            // act + assert
            var result = await _controller.Edit(profileId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");
            
            // act + assert
            var viewResult = (ViewResult)result;
            var model = (UserProfile)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(profileId, model.ProfileID, "ProfileID should match");
            Assert.IsTrue(viewResult.ViewData.ContainsKey("Gender"), "ViewData should contain 'Gender'");
            
            // act + assert
            var selectList = (SelectList)viewResult.ViewData["Gender"];
            Assert.IsNotNull(selectList, "ViewData['Gender'] should be a SelectList");

            // act + assert
            var selectedValue = selectList.SelectedValue?.ToString();
            var expectedValue = model.Gender.ToString();
            Assert.AreEqual(expectedValue, selectedValue, "Selected gender type should match");
        }

        [TestMethod]
        public async Task Edit_2()
        {
            // tests that a record gets updated and verifies the profile is in db and redirection is to Profile
            // for this I'm creating a new record first instead of using a seeded one
            // arrange
            var userId = "testuser123"; 
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var profileId = 4;
            var newProfileEntry = new UserProfile
            {
                ProfileID = profileId,
                Height = 166, 
                Gender = GenderTypes.Male,
                Id = "testuser123"
            };

            _context.ProfileEntries.Add(newProfileEntry);
            await _context.SaveChangesAsync();

            var updateToNewProfileEntry = new UserProfile
            {
                ProfileID = profileId,
                Height = 170, // updated
                Gender = GenderTypes.Male, 
                Id = "testuser123"
            };

            // Detach any existing tracked entity with the same key
            // System.InvalidOperationException: The instance of entity type 'UserProfile' cannot be tracked because another instance with the, AsNoTracking() issue
            var existingEntry = _context.ChangeTracker.Entries<UserProfile>()
                                         .FirstOrDefault(e => e.Entity.ProfileID == profileId);
            if (existingEntry != null)
            {
                _context.Entry(existingEntry.Entity).State = EntityState.Detached;
            }

            // act + assert
            var result = await _controller.Edit(profileId, updateToNewProfileEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Profile", redirectResult.ActionName, "Redirect action name should be 'Profile'");

            // db check
            var updatedEntry = await _context.ProfileEntries
                        .AsNoTracking()  // Use AsNoTracking to avoid tracking conflicts
                        .Where(p => p.ProfileID == profileId && p.Id == userId)
                        .FirstOrDefaultAsync();
            Assert.IsNotNull(updatedEntry, "Updated food entry should not be null");
            Assert.AreEqual(updateToNewProfileEntry.Height, updatedEntry.Height, "Height should match");
        }

        [TestMethod]
        public async Task Edit_3()
        {
            // tests existing profile for invalid model state, height property is missing
            // first I create a new entry for the db, I'm not modifying seeded entries from above
            // arrange
            var userId = "testuser123";
            var profileId = 7; 

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newProfileEntry = new UserProfile
            {
                ProfileID = profileId,
                Height = 156,
                Gender = GenderTypes.Male,
                Id = userId
            };

            // add the new entry to db
            _context.ProfileEntries.Add(newProfileEntry);
            _context.SaveChanges();

            // add invalid model state
            _controller.ModelState.AddModelError("Height", "Required");

            // create invalid entry, Height is missing
            var invalidProfileEntry = new UserProfile
            {
                ProfileID = profileId,
                Gender = GenderTypes.Male,
                Id = userId
            };

            // get current state of profileId 7 in db BEFORE calling controller.Edit()
            UserProfile originalProfileEntry;
            originalProfileEntry = _context.ProfileEntries.Find(profileId);

            // act + assert
            var result = await _controller.Edit(profileId, invalidProfileEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsTrue(viewResult.ViewData.ModelState.ContainsKey("Height"), "ModelState should contain 'Height' error"); // isTrue because db did not change as model passed in was invalid
            Assert.AreEqual("Required", viewResult.ViewData.ModelState["Height"].Errors[0].ErrorMessage, "ModelState error message should be 'Required'");

            // get pre-controller.Edit() profile entry state and compare to current db state to verify that profile entry was not updated
            var currentProfileEntry = _context.ProfileEntries.Find(profileId);
            Assert.IsNotNull(currentProfileEntry, "Profile entry should still exist in the database");

            // Assert that the database value have not changed
            Assert.AreEqual(originalProfileEntry.Height, currentProfileEntry.Height, "Height should not have been updated");
        }

        [TestMethod]
        public async Task Profile()
        {
            // tests that view contains correct profile and weight data
            // also tests that the view pulls in correct services 
            // arrange
            var userId = "testuser123";
            var date = DateTime.Now;

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var bmiValue = 22.5;
            var bmiCategory = "Normal weight";
            _bmiServiceMock.Setup(bs => bs.GetBMI(date, userId)).ReturnsAsync(bmiValue);
            _bmiCategoryMock.Setup(bc => bc.GetBMICategory(bmiValue)).ReturnsAsync(bmiCategory);

            // act + assert
            var result = await _controller.Profile(date);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            var model = (UserProfile)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");

            // act + assert
            var profileEntry = _context.ProfileEntries.FirstOrDefault(p => p.Id == userId);
            var weightEntry = _context.WeightEntries.OrderByDescending(w => w.Timestamp).FirstOrDefault(p => p.Id == userId);

            Assert.IsTrue(viewResult.ViewData.ContainsKey("WeightEntry"), "ViewData should contain 'WeightEntry'");
            Assert.AreEqual(weightEntry, viewResult.ViewData["WeightEntry"]);

            Assert.IsTrue(viewResult.ViewData.ContainsKey("BMIService"), "ViewData should contain 'BMIService'");
            Assert.AreEqual(bmiValue, viewResult.ViewData["BMIService"]);

            Assert.IsTrue(viewResult.ViewData.ContainsKey("BMICategory"), "ViewData should contain 'BMICategory'");
            Assert.AreEqual(bmiCategory, viewResult.ViewData["BMICategory"]);

            Assert.AreEqual(profileEntry.ProfileID, model.ProfileID);
        }
    }
}