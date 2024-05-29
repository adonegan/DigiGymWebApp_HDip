using System.ComponentModel;

namespace DigiGymWebApp_HDip.Models
{
    public class BMICalculatorService
    {
        public double CalculateBMIValue(double weight, double height)
        {
            double pounds2kilos = weight / 2.205;
            double BMIValue = pounds2kilos / Math.Pow(height,2);
            return BMIValue;
        }

        public string CalculateBMICategory(double BMIValue)
        {
   
            if (BMIValue <= 18.5)
            {
                return "Underweight";
            }
            else if (BMIValue >= 18.5 && BMIValue < 24.9)
            {
                return "Normal";
            }
            else if (BMIValue > 25 && BMIValue <= 29)
            {
                return "Overweight";
            } 
            else if (BMIValue > 30)
            {
                return "Obese";
            }
            else
            {
                throw new Exception("Error: Number is out of range");
            }
  
        }

    }
}


