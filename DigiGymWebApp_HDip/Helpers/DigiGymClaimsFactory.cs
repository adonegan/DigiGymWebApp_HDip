using DigiGymWebApp_HDip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DigiGymWebApp_HDip.Helpers
{
    // Create Claims Principle objects, security context for users
    public class DigiGymClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public DigiGymClaimsFactory(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager,IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        // Get user's identity and add new claim to it
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var userIdentity = await base.GenerateClaimsAsync(user);
            userIdentity.AddClaim(new Claim(ClaimTypes.Role, user.UserType.EnumRoleToString()));
            return userIdentity;
        }
    }

}

