using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityNetCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiSecurityController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public ApiSecurityController(IConfiguration configuration, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [Route(template:"Auth")]
        [HttpPost]
        public async Task<IActionResult> TokenAuth(SigninViewModel model)
        {
            //issuer : which server it recevice the token
            var issuer = _configuration["Tokens:Issuer"];
            //audience : In which server it consumes the token
            var audience = _configuration["Tokens:Audience"];
            //key : In which server it gets the token
            var key = _configuration["Tokens:Key"];

            if (ModelState.IsValid)
            {
                var signinResult =
                    await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                if (signinResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Username);
                    if (user != null)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Email , user.Email), 
                            new Claim(JwtRegisteredClaimNames.Jti , user.Id), 
                        };

                        //Secret key needed in the bytes to generate the token
                        var keyBytes = Encoding.UTF8.GetBytes(key);
                        //This will generates symentric key to create the token
                        var theKey = new SymmetricSecurityKey(keyBytes);
                        //This hash a symentric token with the given hasing algorithim
                        var creds = new SigningCredentials(theKey, SecurityAlgorithms.HmacSha256);
                        
                        //this will create a token with the given configuration
                        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.Now.AddMinutes(30), signingCredentials: creds);

                        //This will return the token in the client side
                        return Ok(new {token= new JwtSecurityTokenHandler().WriteToken(token) });
                    }
                }
            }

            return BadRequest();
        }
    }
}