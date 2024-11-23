using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController(ILikesRepository likesRepository) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();

            if(sourceUserId == targetUserId) return BadRequest("You cannot like yourself");

            var exisitingLike = await likesRepository.GetUserLike(sourceUserId, targetUserId);

            if(exisitingLike == null)
            {
                var like = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };
                likesRepository.Addlike(like);
            }
            else
            {
                likesRepository.DeleteLike(exisitingLike);
            }

            if(await likesRepository.SaveChanges()) return Ok();

            return BadRequest("Unable to update like");
        }

        [HttpGet("get-like-list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikes()
        {
            return Ok(await likesRepository.GetCurrentUserLikesId(User.GetUserId()));
        }

        [HttpGet("get-user-likes")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }
    }
}