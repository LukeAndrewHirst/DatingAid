using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
    {
        [HttpGet("get-all-users")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();
            if(users == null) return BadRequest("No users found");

            return Ok(users);
        }

        [HttpGet("get-user-by-username/{username}")]
        public async Task<ActionResult<MemberDto>> GetUserByUserName(string username)
        {
            var user = await userRepository.GetMemberByUsername(username);
            if(user == null) return BadRequest("User not found");

            return Ok(user);
        }

        [HttpPut("update-user")]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(username == null) return BadRequest("Username not found in token");

            var user = await userRepository.GetUserByUsernameAsync(username);
            if(user == null) return BadRequest("Couldnot find user");

            mapper.Map(memberUpdateDto, user);

            if(await userRepository.SaveAsync()) return NoContent();

            return BadRequest("Unable to update member");
        }
    }
}