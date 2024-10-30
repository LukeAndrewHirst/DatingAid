using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository) : BaseApiController
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
            var user = await userRepository.GetMemberAsync(username);
            if(user == null) return BadRequest("User not found");

            return Ok(user);
        }
    }
}