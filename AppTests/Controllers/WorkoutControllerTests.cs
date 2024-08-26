using DigiGymWebApp_HDip.Controllers;
using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace AppTests.Controllers
{
    [TestClass]
    public class WorkoutControllerTests
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
                context.Workouts.AddRange(
                    new Workout
                    {
                        WorkoutID = 1,
                        WorkoutType = WorkoutTypes.Cycle,
                        StartTime = new DateTime(2024, 8, 26, 21, 22, 0),
                        EndTime = new DateTime(2024, 8, 26, 21, 52, 0),
                        EffortLevel = EffortLevels.Low,
                        OtherInfo = "",
                        Date = new DateTime(2024, 8, 26, 23, 00, 0),
                        Id = "testuser123"
                    },
                    new Workout
                    {
                        WorkoutID = 2,
                        WorkoutType = WorkoutTypes.Swim,
                        StartTime = new DateTime(2024, 8, 23, 21, 22, 0),
                        EndTime = new DateTime(2024, 8, 23, 21, 52, 0),
                        EffortLevel = EffortLevels.High,
                        OtherInfo = "",
                        Date = new DateTime(2024, 8, 25, 23, 00, 0),
                        Id = "testuser123"
                    },
                    new Workout
                    {
                        WorkoutID = 3,
                        WorkoutType = WorkoutTypes.Run,
                        StartTime = new DateTime(2024, 8, 24, 19, 22, 0),
                        EndTime = new DateTime(2024, 8, 24, 20, 52, 0),
                        EffortLevel = EffortLevels.Peak,
                        OtherInfo = "",
                        Date = new DateTime(2024, 8, 24, 23, 00, 0),
                        Id = "testuser123"
                    }
                );
                context.SaveChanges();

            }
            _context = new ApplicationDbContext(_options);
        }

        [TestMethod]
        public async Task Create_View()
        {
            // test is view is not null, is a ViewResult and view name is Create
            // arrange
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Create() as ViewResult;
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            var viewResult = (ViewResult)result;
            Assert.AreEqual("Create", viewResult.ViewName, "The view name should be 'Create'");

            // act + assert
            var selectList = result.ViewData["WorkoutType"] as SelectList;
            Assert.IsNotNull(selectList);
            // check selectList has same number of items as enum values
            Assert.AreEqual(Enum.GetValues(typeof(WorkoutTypes)).Cast<WorkoutTypes>().Count(), selectList.Count());

            // act + assert
            var selectListEffort = result.ViewData["EffortLevel"] as SelectList;
            Assert.IsNotNull(selectListEffort);
            // check selectList has same number of items as enum values
            Assert.AreEqual(Enum.GetValues(typeof(EffortLevels)).Cast<EffortLevels>().Count(), selectListEffort.Count());
        }

        [TestMethod]
        public async Task Dates_View()
        {
            // tests that controllers returns viewResult and that correct model properties are returned
            // arrange
            var userId = "testuser123"; 
            var testDate = new DateTime(2024, 8, 26); // make sure seeded data has this date!

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Dates(testDate);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "The result should be a ViewResult.");

            // act + assert
            var model = viewResult.Model as List<Workout>;
            Assert.IsNotNull(model, "The model should be of type List<Workout>.");
            // check that the correct workout entries were retrieved
            Assert.AreEqual(1, model.Count, "Model should contain 1 workout entry for the specified date.");
            Assert.AreEqual(EffortLevels.Low, model.First().EffortLevel, "The retrieved workout entry should be 'Low'.");
        }

        [TestMethod]
        public async Task Create_Post()
        {
            // tests that redirection is to Confirm page, new entry added to db
            // arrange
            var userId = "testuser123";
            var workoutId = 4;
            var workout = new Workout
            {
                WorkoutID = workoutId,
                WorkoutType = WorkoutTypes.Walk,
                StartTime = new DateTime(2024, 8, 21, 18, 32, 0),
                EndTime = new DateTime(2024, 8, 21, 20, 52, 0),
                EffortLevel = EffortLevels.Medium,
                OtherInfo = "",
                Date = new DateTime(2024, 8, 30, 14, 00, 0),
                Id = "testuser123"
            };

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Create(workout);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Confirm", redirectResult.ActionName, "Redirect action name should be 'Confirm'");

            // verify that the Workout record is added to the database
            using (var context = new ApplicationDbContext(_options))
            {
                var savedWorkout = await context.Workouts.FirstOrDefaultAsync(w => w.WorkoutID == workout.WorkoutID);
                Assert.IsNotNull(savedWorkout, "Workout entry should be saved to the database");
                Assert.AreEqual(workout.WorkoutType, savedWorkout.WorkoutType, "Saved workout type should match");
            }
        }

        [TestMethod]
        public async Task Confirm_View()
        {
            // tests that view returned is not null and that model properties passed are correct
            // arrange
            var controller = new WorkoutController(_context, _userManagerMock.Object);
            var workout = new Workout
            {
                WorkoutID = 5,
                WorkoutType = WorkoutTypes.Run,
                StartTime = new DateTime(2024, 7, 21, 18, 32, 0),
                EndTime = new DateTime(2024, 7, 21, 20, 52, 0),
                EffortLevel = EffortLevels.Peak,
                OtherInfo = "",
                Date = new DateTime(2024, 7, 30, 14, 00, 0),
                Id = "testuser123"
            };

            // act + assert
            var result = await controller.Confirm(workout);
            Assert.IsNotNull(result, "It should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "It should be a ViewResult");

            // use cast operator to make IActionResult ViewResult instead
            // do this to access the Model property
            var viewResult = (ViewResult)result;
            // model contains the data the view will render
            Assert.IsNotNull(viewResult.Model, "Model should not be null");
            Assert.IsInstanceOfType(viewResult.Model, typeof(Workout), "Model should be of type Workout");

            // act + assert
            // cast object as instance of Workout class
            var model = (Workout)viewResult.Model;
            // checks that properties of workout object and model passed to the view are the same, this checks that data is passed to the view as expected
            Assert.AreEqual(workout.WorkoutID, model.WorkoutID, "WorkoutID should match");
            Assert.AreEqual(workout.WorkoutType, model.WorkoutType, "WorkoutType should match");
            Assert.AreEqual(workout.StartTime, model.StartTime, "StartTime should match");
            Assert.AreEqual(workout.EndTime, model.EndTime, "EndTime should match");
            Assert.AreEqual(workout.EffortLevel, model.EffortLevel, "EffortLevel should match");
            Assert.AreEqual(workout.OtherInfo, model.OtherInfo, "OtherInfo should match");
            Assert.AreEqual(workout.Date, model.Date, "Date should match");
            Assert.AreEqual(workout.Id, model.Id, "Id should match");
        }

        [TestMethod]
        public async Task Details_1()
        {
            // tests that valid Id returns viewResult with correct model properties
            // arrange
            var userId = "testuser123"; 
            var workoutId = 2; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Details(workoutId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            var model = (Workout)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(workoutId, model.WorkoutID);
            Assert.AreEqual(EffortLevels.High, model.EffortLevel);
        }

        [TestMethod]
        public async Task Details_2()
        {
            // tests that invalid workout id returns Not Found
            // arrange
            var userId = "testuser123"; 
            var invalidWorkoutId = 103; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Details(invalidWorkoutId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "Result should be a NotFoundResult");
        }

        [TestMethod]
        public async Task Details_3()
        {
            // tests that if there's an unauthorised user, result is Not Found
            // arrange
            var validWorkoutId = 1;
            var unauthorizedUserId = "unauthorizedUser"; 

            // mock GetUserId to return a user ID that does not match the one used for the workout entry
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(unauthorizedUserId);
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Details(validWorkoutId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "Result should be a NotFoundResult");
        }

        [TestMethod]
        public async Task Delete_Action()
        {
            // tests that a valid id in this action removes the workout entry and redirects user to Workouts page
            // arrange
            var userId = "testuser123";
            var workoutIdToDelete = 111; // Use a new record for this test instead of previously seeded data

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // create a new context and add test data
            using (var context = new ApplicationDbContext(_options))
            {
                context.Workouts.Add(new Workout
                {
                    WorkoutID = workoutIdToDelete,
                    WorkoutType = WorkoutTypes.Walk,
                    StartTime = new DateTime(2023, 8, 26, 21, 22, 0),
                    EndTime = new DateTime(2023, 8, 26, 21, 52, 0),
                    EffortLevel = EffortLevels.Peak,
                    OtherInfo = "",
                    Date = new DateTime(2023, 8, 26, 23, 00, 0),
                    Id = "testuser123"
                });
                context.SaveChanges();
            }

            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Delete(workoutIdToDelete);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Workouts", redirectResult.ActionName, "Redirect action name should be 'Workouts'");

            // verify that the workout entry was removed from the database
            using (var context = new ApplicationDbContext(_options))
            {
                var workoutEntry = await context.Workouts
                                      .Where(w => w.WorkoutID == workoutIdToDelete && w.Id == userId)
                                      .FirstOrDefaultAsync();
                Assert.IsNull(workoutEntry, "Workout entry should be removed from the database");
            }
        }

        [TestMethod]
        public async Task Edit_1()
        {
            // tests that valid id returns viewResult with correct model and selectList 
            // arrange
            var userId = "testuser123"; 
            var workoutId = 1; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Edit(workoutId);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            // assert that the model is of type Workout and has the correct values
            var model = (Workout)viewResult.Model;
            Assert.IsNotNull(model, "Model should not be null");
            Assert.AreEqual(workoutId, model.WorkoutID, "WorkoutID should match");

            // act + assert
            // verify ViewBag.WorkoutType exists
            Assert.IsTrue(viewResult.ViewData.ContainsKey("WorkoutType"), "ViewData should contain 'WorkoutType'");
            var selectListWorkoutType = (SelectList)viewResult.ViewData["WorkoutType"];
            Assert.IsNotNull(selectListWorkoutType, "ViewData['WorkoutType'] should be a SelectList");

            // check that the selected value is the same as the model's WorkoutType
            var selectedValue1 = selectListWorkoutType.SelectedValue?.ToString();
            var expectedValue1 = model.WorkoutType.ToString();
            Assert.AreEqual(expectedValue1, selectedValue1, "Selected workout type should match");

            // act + assert
            // verify ViewBag.EffortLevel exists
            Assert.IsTrue(viewResult.ViewData.ContainsKey("EffortLevel"), "ViewData should contain 'EffortLevel'");
            var selectListEffort = (SelectList)viewResult.ViewData["EffortLevel"];
            Assert.IsNotNull(selectListEffort, "ViewData['EffortLevel'] should be a SelectList");

            // check that the selected value is the same as the model's EffortLevel
            var selectedValue2 = selectListEffort.SelectedValue?.ToString();
            var expectedValue2 = model.EffortLevel.ToString();
            Assert.AreEqual(expectedValue2, selectedValue2, "Selected effortlevel should match");
        }

        [TestMethod]
        public async Task Edit_2()
        {
            // tests redirection to Details page, that a record gets updated and verifies the new workout entry is in db
            // for this I'm creating a new record first instead of using a seeded one
            // arrange
            var userId = "testuser123";
            var workoutId = 6; // new example WorkoutID for testing, make sure this ID matches the data to be added to db below

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newWorkoutEntry = new Workout
            {
                WorkoutID = workoutId,
                WorkoutType = WorkoutTypes.Cycle,
                StartTime = new DateTime(2024, 6, 17, 21, 22, 0),
                EndTime = new DateTime(2024, 6, 17, 21, 52, 0),
                EffortLevel = EffortLevels.Low,
                OtherInfo = "",
                Date = new DateTime(2024, 6, 17, 23, 00, 0),
                Id = "testuser123"
            };

            var updateToNewWorkoutEntry = new Workout
            {
                WorkoutID = workoutId,
                WorkoutType = WorkoutTypes.Walk, // updated
                StartTime = new DateTime(2024, 8, 26, 21, 22, 0),
                EndTime = new DateTime(2024, 8, 26, 21, 52, 0),
                EffortLevel = EffortLevels.Low,
                OtherInfo = "",
                Date = new DateTime(2024, 8, 26, 23, 00, 0),
                Id = "testuser123"
            };

            // add the new entry to db
            using (var context = new ApplicationDbContext(_options))
            {
                context.Workouts.Add(newWorkoutEntry);
                context.SaveChanges();
            }

            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Edit(workoutId, updateToNewWorkoutEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult), "Result should be a RedirectToActionResult");

            // act + assert
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Details", redirectResult.ActionName, "Redirect action name should be 'Details'");

            // check that the workout entry was updated in the database
            using (var context = new ApplicationDbContext(_options))
            {
                var updatedEntry = await context.Workouts
                                    .Where(w => w.WorkoutID == workoutId && w.Id == userId)
                                    .FirstOrDefaultAsync();
                Assert.IsNotNull(updatedEntry, "Updated workout entry should not be null");
                Assert.AreEqual(updateToNewWorkoutEntry.EffortLevel, updatedEntry.EffortLevel, "EffortLevel should match");
            }
        }

        [TestMethod]
        public async Task Edit_3()
        {
            // tests that an invalid model state returns viewResult and does not update db
            // first I'm creating a new workout entry and not using the seeded entries above
            // arrange
            var userId = "testuser123";
            var workoutId = 7; 

            // mock GetUserId to return the seeded user ID
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var newWorkoutEntry = new Workout
            {
                WorkoutID = workoutId,
                WorkoutType = WorkoutTypes.Run,
                StartTime = new DateTime(2024, 4, 23, 21, 22, 0),
                EndTime = new DateTime(2024, 4, 23, 21, 52, 0),
                EffortLevel = EffortLevels.High,
                OtherInfo = "",
                Date = new DateTime(2024, 4, 23, 23, 00, 0),
                Id = "testuser123"
            };


            // add the new entry to db
            using (var context = new ApplicationDbContext(_options))
            {
                context.Workouts.Add(newWorkoutEntry);
                context.SaveChanges();
            }

            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // add invalid model state
            controller.ModelState.AddModelError("WorkoutType", "Required");

            // create invalid entry, WorkoutType is missing
            var invalidWorkoutEntry = new Workout
            {
                WorkoutID = workoutId,
                StartTime = new DateTime(2024, 4, 23, 21, 22, 0),
                EndTime = new DateTime(2024, 4, 23, 21, 52, 0),
                EffortLevel = EffortLevels.High,
                OtherInfo = "",
                Date = new DateTime(2024, 4, 23, 23, 00, 0),
                Id = "testuser123"
            };

            // get current state of workoutId 7 in db BEFORE calling controller.Edit()
            Workout originalWorkoutEntry;
            using (var context = new ApplicationDbContext(_options))
            {
                originalWorkoutEntry = context.Workouts.Find(workoutId);
            }

            // act + assert
            var result = await controller.Edit(workoutId, invalidWorkoutEntry);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Result should be a ViewResult");

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsTrue(viewResult.ViewData.ModelState.ContainsKey("WorkoutType"), "ModelState should contain 'WorkoutType' error"); // isTrue because db did not change as model passed in was invalid
            Assert.AreEqual("Required", viewResult.ViewData.ModelState["WorkoutType"].Errors[0].ErrorMessage, "ModelState error message should be 'Required'");


            // get pre-controller.Edit() workout entry state and compare to current db state to verify that workout entry was not updated
            using (var context = new ApplicationDbContext(_options))
            {
                var currentWorkoutEntry = context.Workouts.Find(workoutId);
                Assert.IsNotNull(currentWorkoutEntry, "Workout entry should still exist in the database");

                // Assert that the database values have not changed
                Assert.AreEqual(originalWorkoutEntry.EffortLevel, currentWorkoutEntry.EffortLevel, "EffortLevel should not have been updated");
                Assert.AreEqual(originalWorkoutEntry.WorkoutType, currentWorkoutEntry.WorkoutType, "WorkoutType should not have been updated");
            }
        }

        [TestMethod]
        public async Task Workouts_View()
        {
            // tests all records in test db, which are seeded at intialisation + viewResult is returned
            // arrange
            var userId = "testuser123";
            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            var controller = new WorkoutController(_context, _userManagerMock.Object);

            // act + assert
            var result = await controller.Workouts();
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            // act + assert
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "result should be a ViewResult");

            // act + assert
            var model = (List<DateTime>)viewResult.Model;

            Assert.IsNotNull(model, "Expected List<DateTime> model type");
            Assert.AreEqual(3, model.Count, "Model should contain 3 workouts for the user.");
            Assert.AreEqual(new DateTime(2024, 8, 26), model[0].Date, "1st workout should have correct date.");
            Assert.AreEqual(new DateTime(2024, 8, 25), model[1].Date, "2nd workout should have correct date.");
            Assert.AreEqual(new DateTime(2024, 8, 24), model[2].Date, "3rd workout should have correct date.");
        }
    }
}