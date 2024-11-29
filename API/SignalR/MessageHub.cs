using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request.Query["user"];

            if(Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Unable to join group");
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
            
            if(unitOfWork.HasChanges()) await unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();

            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("Could get user");

            if(username == createMessageDto.RecipientUsername.ToLower()) throw new Exception("You cannot message yourself");

            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(sender == null || recipient == null) throw new HubException("Unable to send a meessage, send / recipient not found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content,
                IsRead = false
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if(group != null && group.Connections.Any(u => u.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
                message.IsRead = true;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionForUser(recipient.UserName);
                if(connections != null && connections?.Count != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new {username = sender.UserName, knownAs = sender.KnownAs});
                }
            }

            unitOfWork.MessageRepository.AddMessage(message);

            if(await unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var username = Context.User.GetUsername() ?? throw new Exception("Could not find user");
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection{ConnectionId = Context.ConnectionId, Username = username};

            if(group == null)
            {
                group = new Group{Name = groupName};
                unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await unitOfWork.Complete()) return group;

            throw new HubException("Unable to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group?.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            if(connection != null && group != null)
            {
                unitOfWork.MessageRepository.RemoveConnection(connection);
                if(await unitOfWork.Complete()) return group;
            }

            throw new HubException("Unable to remove group");
        }
    }
}