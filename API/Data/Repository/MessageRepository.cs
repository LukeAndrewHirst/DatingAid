using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
    {
        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Remove(message);
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && x.RecipientDeleted == false),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && x.SenderDeleted == true),
                _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.IsRead == false && x.RecipientDeleted == false)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var query = context.Messages
            .Where(m => m.RecipientUsername == currentUsername && m.RecipientDeleted == false && m.SenderUsername == recipientUsername 
            || m.SenderUsername == currentUsername && m.SenderDeleted == false && m.RecipientUsername == recipientUsername)
            .OrderBy(m => m.MessageSent).AsQueryable();

            var unreadMessages = query.Where(m => m.IsRead == false && m.RecipientUsername == currentUsername).ToList();

            if(unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(m => m.DateRead = DateTime.Now);
            }

            return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
             return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups.Include(mg => mg.Connections).FirstOrDefaultAsync(mg => mg.Name == groupName);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await context.Groups.Include(g => g.Connections).Where(g => g.Connections.Any(gc => gc.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }
    }
}