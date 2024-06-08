using DigiGymWebApp_HDip.Models;

namespace DigiGymWebApp_HDip.Helpers
{
    public static class EnumToUserRoleMapping
    {
        // String value necessary for new Claim creation
        public static string EnumRoleToString(this UserTypes userType)
        {
            switch (userType)
            {
                case UserTypes.Admin:
                    return "Admin";
                case UserTypes.Trainer:
                    return "Trainer";
                case UserTypes.Client:
                    return "Client";
                default:
                    throw new ArgumentOutOfRangeException(nameof(userType), userType, null);
            }

        }
    }
}
