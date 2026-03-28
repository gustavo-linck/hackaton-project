using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace UmbLink.Infrastructure.Identity;

public class CustomUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<AppUser, IdentityRole<Guid>>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrEmpty(user.Name))
            identity.AddClaim(new Claim("display_name", user.Name));
        return identity;
    }
}
