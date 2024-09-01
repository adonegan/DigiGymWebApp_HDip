using DigiGymWebApp_HDip.Controllers;
using DigiGymWebApp_HDip.Data;
using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace AppTests.Controllers
{
    [TestClass]
    public class MessageControllerTests
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

            _context = new ApplicationDbContext(_options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _context.Users.AddRange(
                new ApplicationUser
                {
                    UserName = "testusertrainer@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Id = "testuser123" 
                },
                new ApplicationUser
                {
                    UserName = "testuserclient@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    Id = "testuser145" 
                });
            _context.Messages.AddRange(
                new Message
                {
                    MessageID = 111,
                    ConversationID = 20,
                    SenderID = "sender123",
                    ReceiverID = "receiver124",
                    Content = "Hello World",
                    Timestamp = new DateTime(2024, 8, 27),
                    IsRead = false
                },
                new Message
                {
                    MessageID = 122,
                    ConversationID = 21,
                    SenderID = "sender142",
                    ReceiverID = "receiver143",
                    Content = "Hello Back",
                    Timestamp = new DateTime(2024, 8, 27),
                    IsRead = false
                },
                new Message
                {
                    MessageID = 133,
                    ConversationID = 22,
                    SenderID = "sender333",
                    ReceiverID = "receiver343",
                    Content = "Hello World",
                    Timestamp = new DateTime(2024, 8, 27),
                    IsRead = false
                }
            );
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task Messages_TrainerView()
        {
            // arrange
            // get trainer user from test db
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == "testusertrainer@example.com");

            // create claim statements, package in an identity, assign to a principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Trainer") 
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // mock HttpContext in controller and set the user property there
            var controller = new MessageController(_userManagerMock.Object, _context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
                }
            };

            // act
            var result = await controller.Messages();

            // assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "Result should be a ViewResult");
            Assert.AreEqual("TrainerMessages", viewResult.ViewName, "Should be TrainerMessages");
        }

        [TestMethod]
        public async Task Messages_ClientView()
        {
            // arrange
            // get trainer user from test db
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == "testuserclient@example.com");

            // create claim statements, package in an identity, assign to a principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, "Client") 
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // mock HttpContext in controller and set the user property there
            var controller = new MessageController(_userManagerMock.Object, _context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
                }
            };

            // act
            var result = await controller.Messages();

            // assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult, "Result should be a ViewResult");
            Assert.AreEqual("ClientMessages", viewResult.ViewName, "Should be ClientMessages");
        }

        [TestMethod]
        public async Task Create_View()
        { 
            // arrange
            var convo_id = 0;
            var controller = new MessageController(_userManagerMock.Object, _context);

            // act + assert
            var result = await controller.Create(convo_id) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Create", result.ViewName, "Name is Create");

            Assert.IsNull(result.ViewData["ConversationInfo"]);
        }
    }
}