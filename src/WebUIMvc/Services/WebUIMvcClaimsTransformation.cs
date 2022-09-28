using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace WebUIMvc.Services;

public class WebUIMvcClaimsTransformation : IClaimsTransformation
{

    private readonly IApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
  
    public WebUIMvcClaimsTransformation(IApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        //just pick what you need and delete the rest of the DI
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;

        // get whatever claims you need from the _dbContext or from _userManager here and add them 

        //claimsIdentity.AddClaim(new Claim("roles", [whateva!]));
        return Task.FromResult(principal);
    }
}