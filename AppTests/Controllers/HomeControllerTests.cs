using DigiGymWebApp_HDip.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppTests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        // Test that view is not null

        [TestMethod]
        public void Index_View()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

    }
}