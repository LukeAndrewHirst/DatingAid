using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            using var hmac = new HMACSHA512();

            if(await UserExisits(registerDto.Username)) return BadRequest("username is already taken");

            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasshwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasshwordSalt = hmac.Key
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                PhotoUrl = null,
                Token = tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.UserName == loginDto.Username.ToLower());
            if(user == null) return Unauthorized("Invalid login credentials");

            using var hmac = new HMACSHA512(user.PasshwordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i = 0; i < computedHash.Length; i++)
            {
                if(computedHash[i] != user.PasshwordHash[i]) return Unauthorized("Invalid login credentials");
            }

            return new UserDto
            {
                Username = user.UserName,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                Token = tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExisits(string username)
        {
            return await context.Users.AnyAsync(u => u.UserName.ToLower() == username.ToLower());
        }
    }
}