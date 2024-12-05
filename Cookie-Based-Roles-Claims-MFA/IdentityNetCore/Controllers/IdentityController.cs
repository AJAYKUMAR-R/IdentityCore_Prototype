using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityNetCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityNetCore.Controllers;

public class IdentityController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    RoleManager<IdentityRole> roleManager)
    : Controller
{
    public Task<IActionResult> Signup()
    {
        var model = new SignupViewModel { Role = "Member" };
        return Task.FromResult<IActionResult>(View(model));
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (!await roleManager.RoleExistsAsync(model.Role))
            {
                var role = new IdentityRole { Name = model.Role };
                var roleResult = await roleManager.CreateAsync(role);
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(s => s.Description);
                    ModelState.AddModelError("Role", string.Join(",", errors));
                    return View(model);
                }
            }


            if (await userManager.FindByEmailAsync(model.Email) == null)
            {
                var user = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var claim = new Claim("Department", model.Department);
                    await userManager.AddClaimAsync(user, claim);
                    await userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction("Signin");
                }

                ModelState.AddModelError("Signup", string.Join("", result.Errors.Select(x => x.Description)));
                return View(model);
            }
        }

        return View(model);
    }

    
    public async Task<IActionResult> MFASetup()
    {
        const string provider = "aspnetidentity";
        var user = await userManager.GetUserAsync(User);
        //reseting the two factor auth because it will bounded to the user in the db
        //before enbeling just reseting will recreating new if it has any : SELECT * FROM AspNetUserTokens 
        await userManager.ResetAuthenticatorKeyAsync(user);

        //This will return the token to scan it in the app once the 
        //they scan it then the next time they login it will ask for QRCode or Code
        //This table will hold the token details in this table :SELECT * FROM AspNetUserTokens 
        var token = await userManager.GetAuthenticatorKeyAsync(user);

        // The below line creates a QR code for Microsoft Authenticator.
        var qrCodeUrl = $"otpauth://totp/{provider}:{user.Email}?secret={token}&issuer={provider}&digits=6";

        // use the below code for Google Authenticator;
        // var qrCodeUrl = $"otpauth://totp/{user.Email}?secret={token}&issuer={provider}&digits=6";


        var model = new MFAViewModel { Token = token, QRCodeUrl = qrCodeUrl };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MFASetup(MFAViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.GetUserAsync(User);
            //it will receive the code and providers and update the flag in this table : SELECT * FROM dbo.AspNetUsers
            //once that is enabled so when ever the login has happend need to provide the code
            var succeeded = await userManager.VerifyTwoFactorTokenAsync(user,
                userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);
            if (succeeded)
                await userManager.SetTwoFactorEnabledAsync(user, true);
            else
                ModelState.AddModelError("Verify", "Your MFA code could not be validated.");
        }

        return View(model);
    }

    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded) return RedirectToAction("Signin");

        return new NotFoundResult();
    }

    public IActionResult Signin()
    {
        return View(new SigninViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Signin(SigninViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result =
                //Lockout information will be stored in this table : SELECT * FROM dbo.AspNetUsers 
                //Lockout Enabled , Lockout End , AccessFailedCount : These are the fields which hold the information
                await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe,true);
            if (result.RequiresTwoFactor) return RedirectToAction("MFACheck");

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    ModelState.AddModelError("login", "account got locked for 3 min due invalid attempt");
                else if (!result.IsNotAllowed)
                    ModelState.AddModelError("Login", "Account Not Allowed");
                else
                    ModelState.AddModelError("Login", "Login Failed");
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
              
        }

        return View(model);
    }

    public IActionResult MFACheck()
    {
        return View(new MNFACheckViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> MFACheck(MNFACheckViewModel model)
    {
        if (ModelState.IsValid)
        {
            //This will compare the code and let the user to login once the code is valid
            var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.Code,false, false);
            if (result.Succeeded) return RedirectToAction("Index", "Home", null);
        }

        return View(model);
    }

    public Task<IActionResult> AccessDenied()
    {
        return Task.FromResult<IActionResult>(View());
    }

    public async Task<IActionResult> Signout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Signin");
    }
}
