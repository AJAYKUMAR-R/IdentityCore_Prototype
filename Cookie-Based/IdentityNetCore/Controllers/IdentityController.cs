using System.Linq;
using System.Threading.Tasks;
using IdentityNetCore.Models;
using IdentityNetCore.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityNetCore.Controllers;

public class IdentityController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly SignInManager<IdentityUser> _signInManager;

    private readonly UserManager<IdentityUser> _userManager;

    public IdentityController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    public Task<IActionResult> Signup()
    {
        var model = new SignupViewModel();
        return Task.FromResult<IActionResult>(View(model));
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (ModelState.IsValid)
            if (await _userManager.FindByEmailAsync(model.Email) == null)
            {
                var user = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                //Selecting the user from the db to get the details of the user with user.id
                user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    //This will generate the token for the email when we pass the userDetails with the userID
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    if (result.Succeeded)
                    {
                        var confirmationLink =
                            Url.ActionLink("ConfirmEmail", "Identity", new { userId = user.Id, token });
                        //The link which routest to the ConfirmEmail action will send with the token 
                        //So when the user click the link with the given url he will be redirected to Confirmail with the token 
                        await _emailSender.SendEmailAsync("info@mydomain.com", user.Email, "Confirm your email address",
                            confirmationLink);

                        return RedirectToAction("Signin");
                    }
                }

                ModelState.AddModelError("Signup", string.Join("", result.Errors.Select(x => x.Description)));
                return View(model);
            }

        return View(model);
    }


    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            //User with the userID and token will got confirmed by this method
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) return RedirectToAction("Signin");
        }

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
            //THe remember me flag will know the user as long as the cookie is not expired
            var result =
                await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
            if (result.Succeeded)
                return RedirectToAction("Signin");
            ModelState.AddModelError("Login", "Cannot login.");
        }

        return View(model);
    }

    //This method will accessed by the other apps without CORS and with CORS ( => 1) Role-Claims-MFA
    //This application has been configured for the CROS . as if this need to send the data
    public IActionResult ReturnCROSUIS()
    {
        return View("ResultCORS");
    }



    public IActionResult AccessDenied()
    {
        return RedirectToAction("Signin");
    }
}