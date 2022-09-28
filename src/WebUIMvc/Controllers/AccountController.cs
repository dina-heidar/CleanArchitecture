using CleanArchitecture.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebUIMvc.Controllers;
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string scheme, string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        // validate returnUrl 
        if (Url.IsLocalUrl(returnUrl) == false)
        {
            // user might have clicked on a malicious link - should be logged
            throw new Exception("invalid return URL");
        }

        // start challenge and roundtrip the return URL and scheme 
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(ExternalLoginCallback)),
            Items =
                {
                    { "returnUrl", returnUrl },
                    { "scheme", scheme },
                    {"LoginProvider",scheme }
                }
        };

        return Challenge(props, scheme);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
    {
        //User.Claims to see incoming claims from ADFS 
        if (Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        // if you are adding claims to the asp.identity tables do it here with _signManager
        // issue authentication cookie for user

        //var info = await _signInManager.GetExternalLoginInfoAsync();
        //await _signInManager.SignInWithClaimsAsync(user, localSignInProps, additionalLocalClaims);
        return RedirectToAction("Index", "Home");
    }
}
