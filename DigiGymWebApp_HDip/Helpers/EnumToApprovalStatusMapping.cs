using DigiGymWebApp_HDip.Models;

namespace DigiGymWebApp_HDip.Helpers
{
    public class EnumToApprovalStatusMapping
    {
        public static string ToStatus(ApprovalStatuses approvalStatus)
        {
            //Not in use yet
            switch (approvalStatus) 
            {
                case ApprovalStatuses.Approved:
                    return "Approved";
                case ApprovalStatuses.Rejected:
                    return "Rejected";
                default:
                    throw new ArgumentOutOfRangeException(nameof(approvalStatus), approvalStatus, null);
            }

        }
    }
}






