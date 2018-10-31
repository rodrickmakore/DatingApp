using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dating.API.Data;
using Dating.API.Dtos;
using Dating.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dating.API.Controllers {

    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController (IAuthRepository repo, IConfiguration config) {
            _repo = repo;
            _config = config;
        }

        [HttpGet]

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserForRegisterDto userForRegisterDto) {

            // validate request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower ();

            if (await _repo.UserExists (userForRegisterDto.Username))
                return BadRequest ("Username already exists");

            var userToCreate = new User () {
                Username = userForRegisterDto.Username
            };

            var createUser = await _repo.Register (userToCreate, userForRegisterDto.Password);

            return StatusCode (201);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserForLoginDto userForLoginDto) {
            var userFromRepo = await _repo.Login (userForLoginDto.Username.ToLower (), userForLoginDto.Password);
            if (userFromRepo == null)
                return Unauthorized ();

            // Our token is going to contain two claims, one for the Id and the other for the Username
            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString ()),
                new Claim (ClaimTypes.Name, userFromRepo.Username)
            };

            // The method creates a security key
            var key = new SymmetricSecurityKey (Encoding.UTF8
                .GetBytes (_config.GetSection ("AppSettings:Token").Value));

            // Encrypt the key with a hashing algorithym 
            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);

            // Creating the token
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity (claims),
                Expires = DateTime.Now.AddDays (1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler ();

            var token = tokenHandler.CreateToken (tokenDescriptor);

            return Ok (new {
                token = tokenHandler.WriteToken (token)
            });

        }

    }
}