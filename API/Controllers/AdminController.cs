using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AdminController(IAdminRepository adminRepository, IUnitOfWork unitOfWork, IPhotoService photoService) : BaseApiController
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
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await unitOfWork.PhotoRepository.GetUnapprovedPhotos();

            return Ok(photos);
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) return BadRequest("Could not find photo");
            
            photo.IsApproved = true;

            var user = await unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            if (user == null) return BadRequest("Could not find user");

            if (!user.Photos.Any(p => p.IsMain)) photo.IsMain = true;
            
            await unitOfWork.Complete();
            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if (photo == null) return BadRequest("Could not find photo");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Result == "ok")
                {
                    unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
            
            await unitOfWork.Complete();
            return Ok();
        }
    }
}