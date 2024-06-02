using DigiGymWebApp_HDip.Models;

namespace DigiGymWebApp_HDip.Helpers
{
    public class EnumToUserRoleMapping
    {
        public static string ToRole(UserType userType)
        {
            switch (userType) 
            {
                case UserType.Admin:
                    return "Admin";
                case UserType.Trainer:
                    return "Trainer";
                case UserType.Client:
                    return "Client";
                default:
                    throw new ArgumentOutOfRangeException(nameof(userType), userType, null);
            }

        }
    }
}
