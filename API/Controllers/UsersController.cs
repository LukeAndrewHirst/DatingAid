using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        [HttpGet("get-all-users")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            userParams.CurrentUsername = User.GetUsername();
            
            var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            if(users == null) return BadRequest("No users found");

            Response.AddPaginationHeader(users);

            return Ok(users);
        }
        
        [HttpGet("get-user-by-username/{username}")]
        public async Task<ActionResult<MemberDto>> GetUserByUserName(string username)
        {
            var currentUsername = User.GetUsername();

            return await unitOfWork.UserRepository.GetMemberByUsername(username, isCurrentUser: currentUsername == username);
        }

        [HttpPut("update-user")]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return BadRequest("Could not find user");

            mapper.Map(memberUpdateDto, user);

            if(await unitOfWork.Complete()) return NoContent();

            return BadRequest("Unable to update member");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return BadRequest("Could not find user");

            var result = await photoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            user.Photos.Add(photo);

            if(await unitOfWork.Complete()) return CreatedAtAction(nameof(GetUserByUserName), new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

            return BadRequest("Unable to upload new photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId) 
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return BadRequest("User not found");

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if(photo == null || photo.IsMain) return BadRequest("Unable to set main photo");

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if(await unitOfWork.Complete()) return NoContent();

            return BadRequest("Unable to set main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if(user == null) return BadRequest("User not found");

            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if(photo == null || photo.IsMain) return BadRequest("Unable to delete photo, this is the main photo of the users profile");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if(await unitOfWork.Complete()) return Ok();

            return BadRequest("Unable to delete photo");
        }
    }
}