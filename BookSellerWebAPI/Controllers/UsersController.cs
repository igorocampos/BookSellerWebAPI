using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookSellerWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> UserManager;
        private readonly SignInManager<IdentityUser> SignInManager;
        private readonly IConfiguration Configuration;

        public UsersController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            Configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] UserInfo model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = model.UserName, Email = model.UserName };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
                return BuildToken(model);
            else
                return BadRequest(result);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await SignInManager.PasswordSignInAsync(userInfo.UserName, userInfo.Password,
                 isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
                return BuildToken(userInfo);
            else
                return BadRequest("Invalid login.");
        }

        private UserToken BuildToken(UserInfo userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tokens will expire in 1 hour
            var expiration = DateTime.UtcNow.AddHours(1);

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return new UserToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
