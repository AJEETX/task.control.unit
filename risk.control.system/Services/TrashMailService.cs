using System.Text.Json.Serialization;
using System.Text.Json;

using risk.control.system.Models;
using risk.control.system.Data;
using Microsoft.EntityFrameworkCore;

namespace risk.control.system.Services
{
    public interface ITrashMailService
    {
        Task<IEnumerable<TrashMessage>> GetTrashMessages(string userEmail);
        Task<int> TrashDelete(List<long> messages, long userId);
        Task<TrashMessage> GetTrashMessagedetail(long messageId, string userEmail);
        Task<int> TrashDetailsDelete(long id, string userEmail);

    }
    public class TrashMailService : ITrashMailService
    {
        private readonly JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
        private readonly ApplicationDbContext _context;

        public TrashMailService(ApplicationDbContext Context)
        {
            _context = Context;
        }

        public async Task<TrashMessage> GetTrashMessagedetail(long messageId, string userEmail)
        {
            var userMailbox = _context.Mailbox
                .Include(m => m.Trash)
                .FirstOrDefault(c => c.Name == userEmail);

            var userMessage = userMailbox.Trash.FirstOrDefault(c => c.TrashMessageId == messageId);
            userMessage.Read = true;
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return userMessage;
        }

        public async Task<IEnumerable<TrashMessage>> GetTrashMessages(string userEmail)
        {
            var userMailbox = _context.Mailbox.Include(m => m.Trash).FirstOrDefault(c => c.Name == userEmail);
            return userMailbox.Trash.OrderByDescending(o => o.SendDate).ToList();
        }

        public async Task<int> TrashDelete(List<long> messages, long userId)
        {
            var userMailbox = _context.Mailbox
                           .Include(m => m.Trash)
                           .Include(m => m.Deleted)
                           .FirstOrDefault(c => c.ApplicationUserId == userId);

            var userTrashMails = userMailbox.Trash.Where(d => messages.Contains(d.TrashMessageId)).ToList();

            if (userTrashMails is not null && userTrashMails.Count > 0)
            {
                foreach (var message in userTrashMails)
                {
                    message.MessageStatus = MessageStatus.TRASHDELETED;
                    userMailbox.Trash.Remove(message);
                    if (message.Attachment?.Length > 0)
                    {
                        //TO-DO
                    }
                    var jsonMessage = JsonSerializer.Serialize(message, options);
                    DeletedMessage deletedMessage = JsonSerializer.Deserialize<DeletedMessage>(jsonMessage, options);
                    userMailbox.Deleted.Add(deletedMessage);
                }
            }
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return rows;
        }

        public async Task<int> TrashDetailsDelete(long id, string userEmail)
        {
            var userMailbox = _context.Mailbox
               .Include(m => m.Trash)
               .FirstOrDefault(c => c.Name == userEmail);

            var userTrashMessage = userMailbox.Trash.FirstOrDefault(c => c.TrashMessageId == id);

            if (userTrashMessage is not null)
            {
                userTrashMessage.MessageStatus = MessageStatus.TRASHDELETED;
                userMailbox.Trash.Remove(userTrashMessage);
                var jsonMessage = JsonSerializer.Serialize(userTrashMessage, options);
                DeletedMessage trashMessage = JsonSerializer.Deserialize<DeletedMessage>(jsonMessage, options);
                userMailbox.Deleted.Add(trashMessage);
            }

            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return rows;
        }
    }
}
