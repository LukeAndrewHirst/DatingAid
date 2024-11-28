using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController(IUnitOfWork unitOfWork) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();

            if(sourceUserId == targetUserId) return BadRequest("You cannot like yourself");

            var exisitingLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);

            if(exisitingLike == null)
            {
                var like = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };
                unitOfWork.LikesRepository.Addlike(like);
            }
            else
            {
                unitOfWork.LikesRepository.DeleteLike(exisitingLike);
            }

            if(await unitOfWork.Complete()) return Ok();

            return BadRequest("Unable to update like");
        }

        [HttpGet("get-like-list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikes()
        {
            return Ok(await unitOfWork.LikesRepository.GetCurrentUserLikesId(User.GetUserId()));
        }

        [HttpGet("get-user-likes")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);

            return Ok(users);
        }
    }
}