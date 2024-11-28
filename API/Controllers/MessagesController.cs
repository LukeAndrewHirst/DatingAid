using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
    {
        [HttpPost("create-message")]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot message yourself");

            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(sender == null || recipient == null) return BadRequest("Unable to send message");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            unitOfWork.MessageRepository.AddMessage(message);

            if(await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet("get-messages-for-user")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("delete-message/{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await unitOfWork.MessageRepository.GetMessage(id);

            if(message == null) return BadRequest("Could not find message to delete");
            if(message.SenderUsername != username || message.RecipientUsername != username) return Forbid();

            if(message.SenderDeleted == true) message.RecipientDeleted = true;
            if(message.RecipientDeleted == true) message.RecipientDeleted = true;

            if(message is {SenderDeleted: true, RecipientDeleted: true})
            {
                unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if(await unitOfWork.Complete()) return Ok();

            return BadRequest("Unable to delete message");
        }
    }
}