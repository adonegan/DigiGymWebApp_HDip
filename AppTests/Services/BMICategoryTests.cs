using DigiGymWebApp_HDip.Services;

namespace AppTests.Services
{
    [TestClass]
    public class BMICategoryTests
    {
        private BMICategory _bmiCategoryService;

        // setup
        [TestInitialize]    
        public void Setup()
        {
            _bmiCategoryService = new BMICategory(null); // don't want context here
        }

        [TestMethod]
        public async Task Get_Underweight()
        {
            // act
            var result = await _bmiCategoryService.GetBMICategory(17.5);

            // assert
            Assert.AreEqual("Underweight", result, "Underweight should be anything less than 18.5");
        }

        [TestMethod]
        public async Task Get_NormalWeight()
        {
            // act
            var result = await _bmiCategoryService.GetBMICategory(20);

            // assert
            Assert.AreEqual("Normal", result, "Normal should be between 18.5 and 24.9");
        }

        [TestMethod]
        public async Task Get_Overweight()
        {
            // act
            var result = await _bmiCategoryService.GetBMICategory(26);

            // assert
            Assert.AreEqual("Overweight", result, "Overweight should between 25 and 29.9");
        }

        [TestMethod]
        public async Task Get_Obese()
        {
            // act
            var result = await _bmiCategoryService.GetBMICategory(33);

            // assert
            Assert.AreEqual("Obese", result, "Obese should be anything over 30");
        }

        [TestMethod]
        public async Task Get_Zero()
        {
            // act
            var result = await _bmiCategoryService.GetBMICategory(0);

            // assert
            Assert.AreEqual("Add Weight and Height", result, "Zero should prompt the user to add height and weight details");
        }
    }
}