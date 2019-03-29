using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CO.MVC.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {

        public IConfiguration Configuration { get; }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> GenerateToken([FromBody] string email)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    //var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    //if (result.Succeeded)
                    //{

                    var claims = new[]
                    {
              new Claim(JwtRegisteredClaimNames.Sub, user.Email),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(Configuration["Tokens:Issuer"],
                      Configuration["Tokens:Audience"],
                      claims,
                      expires: DateTime.Now.AddDays(30),
                      signingCredentials: creds);

                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                    //}
                }
            }

            return BadRequest("Could not create token");
        }

    }
}