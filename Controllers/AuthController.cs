using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LmsApp.API.Data;
using LmsApp.API.Dtos;
using LmsApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LmsApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        public AuthController(IConfiguration config, IMapper mapper, 
            UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new {controller = "Users", 
                    id = userToCreate.Id}, userToReturn);
            }

            return BadRequest("Username already exists! Please choose a new one");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // Check if we have a user and if their username and password match what is in the database           
            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);

            if (user == null)
                return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if (result.Succeeded)
            {        
                var appUser = _mapper.Map<PhotoForNavUpdateDto>(user);

                return Ok(new
                {
                    token = GenerateJwtToken(user).Result,
                    user = appUser
                });
            }  

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                // Building up token with claims

                // Users Id
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // Users username
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // In order to make sure the token is a valid token when it comes back, the server needs to sign
            // the token. "AppSettings:Token" is declared in appsettings.json and should be a massive, 
            // randomly generated string
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create the token and pass in claims, give it expirey date, and pass in signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}