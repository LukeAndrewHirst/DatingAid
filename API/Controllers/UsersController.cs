using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(DataContext context) : ControllerBase
    {
        [HttpGet("get-all-users")]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await context.Users.ToListAsync();
            if(users == null) return BadRequest("No users found");

            return Ok(users);
        }

        [HttpGet("get-user-by-id/{Id}")]
        public async Task<ActionResult<AppUser>> GetUsers(int Id)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if(user == null) return BadRequest("User not found");

            return Ok(user);
        }
    }
}