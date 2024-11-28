using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AdminController(IAdminRepository adminRepository, IUnitOfWork unitOfWork) : BaseApiController
    {
        [Authorize(Policy="RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await adminRepository.GetUsersWithRoles();
            if(users == null) return BadRequest("No users found");

            return Ok(users);
        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if(string.IsNullOrEmpty(roles)) return BadRequest("At least one role should be selected");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if(user == null) return BadRequest("User not found");

            var userRoles = await adminRepository.GetUserRoles(user);

            var result = await adminRepository.AddToRoles(user, selectedRoles, userRoles);
            if(!result.Succeeded) return BadRequest("Unable to add roles to user");

            result = await adminRepository.RemoveFromRoles(user, selectedRoles, userRoles);
            if(!result.Succeeded) return BadRequest("Unable to remove roles from user");

            return Ok(await adminRepository.GetUserRoles(user));
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Only Admins or Moderators can see this");
        }
    }
}