using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExisits(registerDto.Username)) return BadRequest("username is already taken");
            
            var user = mapper.Map<AppUser>(registerDto);

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if(!result.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                PhotoUrl = null,
                Token = await tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.NormalizedUserName == loginDto.Username.ToUpper());
            if(user == null) return Unauthorized("Invalid login credentials");

            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if(!result) return Unauthorized();

            return new UserDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                Token = await tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExisits(string username)
        {
            return await userManager.Users.AnyAsync(u => u.NormalizedUserName == username.ToUpper());
        }
    }
}