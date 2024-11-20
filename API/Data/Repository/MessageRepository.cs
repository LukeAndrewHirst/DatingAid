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
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && m.SenderDeleted == false),
                _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == DateTime.Parse("0001-01-01 00:00:00.0000000") 
                    && m.RecipientDeleted == false)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await context.Messages.Include(m => m.Sender).ThenInclude(p => p.Photos).Include(m => m.Recipient).ThenInclude(p => p.Photos)
            .Where(m => m.RecipientUsername == currentUsername && m.RecipientDeleted == false && m.SenderUsername == recipientUsername 
            || m.SenderUsername == currentUsername && m.SenderDeleted == false && m.RecipientUsername == recipientUsername).OrderBy(m => m.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == DateTime.Parse("0001-01-01 00:00:00.0000000") && m.RecipientUsername == currentUsername).ToList();

            if(unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(m => m.DateRead = DateTime.Now);
                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() >0;
        }
    }
}