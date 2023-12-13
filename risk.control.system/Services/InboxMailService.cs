using System.Text.Json.Serialization;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Models;

namespace risk.control.system.Services
{
    public interface IInboxMailService
    {
        Task<IEnumerable<InboxMessage>> GetInboxMessages(string userEmail);
        Task<int> InboxDelete(List<long> messages, long userId);
        Task<InboxMessage> GetInboxMessagedetail(long messageId, string userEmail);
        Task<OutboxMessage> GetInboxMessagedetailReply(long messageId, string userEmail, string actiontype);
        Task<int> InboxDetailsDelete(long id, string userEmail);
        Task<bool> SendReplyMessage(OutboxMessage contactMessage, string userEmail, IFormFile? messageDocument);
    }
    public class InboxMailService : IInboxMailService
    {
        private readonly JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
        private readonly ApplicationDbContext _context;

        public InboxMailService(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<IEnumerable<InboxMessage>> GetInboxMessages(string userEmail)
        {
            var userMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == userEmail);
            return userMailbox.Inbox.OrderByDescending(o => o.SendDate).ToList();
        }

        public async Task<InboxMessage> GetInboxMessagedetail(long messageId, string userEmail)
        {
            var userMailbox = _context.Mailbox
                .Include(m => m.Inbox)
                .FirstOrDefault(c => c.Name == userEmail);

            var userMessage = userMailbox.Inbox.FirstOrDefault(c => c.InboxMessageId == messageId);
            userMessage.Read = true;

            _context.Mailbox.Attach(userMailbox);
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return userMessage;
        }

        public async Task<OutboxMessage> GetInboxMessagedetailReply(long messageId, string userEmail, string actiontype)
        {
            var userMailbox = _context.Mailbox
                .Include(m => m.Inbox)
                .FirstOrDefault(c => c.Name == userEmail);

            var userMessage = userMailbox.Inbox.FirstOrDefault(c => c.InboxMessageId == messageId);

            var replyRawMessage = "<br />" + "<hr />" + "From: "+userMessage.SenderEmail + "<br />" + "<hr />" + "Sent:" + userMessage.SendDate + "<br />" + "<hr />" + userMessage.RawMessage;

            var userReplyMessage = new OutboxMessage
            {
                ReceipientEmail = userMessage.SenderEmail,
                SenderEmail = userEmail,
                Subject = actiontype + " :" + userMessage.Subject,
                Attachment = userMessage.Attachment,
                AttachmentName = userMessage.AttachmentName,
                Created = userMessage.Created,
                Extension = userMessage.Extension,
                FileType = userMessage.FileType,
                Message = userMessage.Message,
                RawMessage = replyRawMessage,
                Read = false,

            };
            return userReplyMessage;
        }

        public async Task<int> InboxDelete(List<long> messages, long userId)
        {
            var userMailbox = _context.Mailbox
               .Include(m => m.Inbox)
               .Include(m => m.Trash)
               .FirstOrDefault(c => c.ApplicationUserId == userId);

            var userInboxMails = userMailbox.Inbox.Where(d => messages.Contains(d.InboxMessageId)).ToList();

            if (userInboxMails is not null && userInboxMails.Count > 0)
            {
                foreach (var message in userInboxMails)
                {
                    message.MessageStatus = MessageStatus.TRASHED;
                    userMailbox.Inbox.Remove(message);
                    var trashMessage = new TrashMessage
                    {
                        Attachment = message.Attachment,
                        AttachmentName = message.AttachmentName,
                        Created = message.Created,
                        Extension = message.Extension,
                        FileType = message.FileType,
                        Message = message.Message,
                        MessageStatus = MessageStatus.TRASHED,
                        Priority = message.Priority,
                        Read = message.Read,
                        ReceipientEmail = message.ReceipientEmail,
                        SendDate = message.SendDate,
                        Subject = message.Subject,
                        SenderEmail = message.SenderEmail,
                        Updated = message.Updated,
                        UpdatedBy = message.UpdatedBy,
                    };
                    userMailbox.Trash.Add(trashMessage);
                }
            }
            _context.Mailbox.Update(userMailbox);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> InboxDetailsDelete(long id, string userEmail)
        {
            var userMailbox = _context.Mailbox
                .Include(m => m.Inbox)
                .Include(m => m.Trash)
                .FirstOrDefault(c => c.Name == userEmail);

            var message = userMailbox.Inbox.FirstOrDefault(c => c.InboxMessageId == id);

            if (message is not null)
            {
                message.MessageStatus = MessageStatus.TRASHED;
                userMailbox.Inbox.Remove(message);
                var trashMessage = new TrashMessage
                {
                    Attachment = message.Attachment,
                    AttachmentName = message.AttachmentName,
                    Created = message.Created,
                    Extension = message.Extension,
                    FileType = message.FileType,
                    Message = message.Message,
                    MessageStatus = MessageStatus.TRASHED,
                    Priority = message.Priority,
                    Read = message.Read,
                    ReceipientEmail = message.ReceipientEmail,
                    SendDate = message.SendDate,
                    Subject = message.Subject,
                    SenderEmail = message.SenderEmail,
                    Updated = message.Updated,
                    UpdatedBy = message.UpdatedBy,
                };
                userMailbox.Trash.Add(trashMessage);
            }

            _context.Mailbox.Update(userMailbox);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> SendReplyMessage(OutboxMessage contactMessage, string userEmail, IFormFile? messageDocument)
        {
            var userMailbox = _context.Mailbox.Include(m => m.Sent).Include(m => m.Outbox).FirstOrDefault(c => c.Name == userEmail);

            var recepientMailbox = _context.Mailbox.FirstOrDefault(c => c.Name == contactMessage.ReceipientEmail);

            contactMessage.Read = false;
            contactMessage.SendDate = DateTime.UtcNow;
            contactMessage.Updated = DateTime.UtcNow;
            contactMessage.UpdatedBy = userEmail;
            contactMessage.SenderEmail = userEmail;
            if (recepientMailbox is not null)
            {
                //add to sender's sent box
                var jsonMessage = JsonSerializer.Serialize(contactMessage, options);
                SentMessage sentMessage = JsonSerializer.Deserialize<SentMessage>(jsonMessage, options);

                if (messageDocument is not null)
                {
                    var messageDocumentFileName = Path.GetFileNameWithoutExtension(messageDocument.FileName);
                    var extension = Path.GetExtension(messageDocument.FileName);

                    sentMessage.Document = messageDocument;
                    using var dataStream = new MemoryStream();
                    await sentMessage.Document.CopyToAsync(dataStream);
                    sentMessage.Attachment = dataStream.ToArray();
                    sentMessage.FileType = messageDocument.ContentType;
                    sentMessage.Extension = extension;
                    sentMessage.AttachmentName = messageDocumentFileName;
                }
                userMailbox.Sent.Add(sentMessage);
                _context.Mailbox.Attach(userMailbox);
                _context.Mailbox.Update(userMailbox);

                //add to receiver's inbox
                InboxMessage inboxMessage = JsonSerializer.Deserialize<InboxMessage>(jsonMessage, options);

                if (messageDocument is not null)
                {
                    var messageDocumentFileName = Path.GetFileNameWithoutExtension(messageDocument.FileName);
                    var extension = Path.GetExtension(messageDocument.FileName);

                    inboxMessage.Document = messageDocument;
                    using var dataStream = new MemoryStream();
                    await inboxMessage.Document.CopyToAsync(dataStream);
                    inboxMessage.Attachment = dataStream.ToArray();
                    inboxMessage.FileType = messageDocument.ContentType;
                    inboxMessage.Extension = extension;
                    inboxMessage.AttachmentName = messageDocumentFileName;
                }
                recepientMailbox.Inbox.Add(inboxMessage);
                _context.Mailbox.Attach(recepientMailbox);
                _context.Mailbox.Update(recepientMailbox);

                var rows = await _context.SaveChangesAsync();

                return true;
            }
            else
            {
                var jsonMessage = JsonSerializer.Serialize(contactMessage, options);
                OutboxMessage outboxMessage = JsonSerializer.Deserialize<OutboxMessage>(jsonMessage, options);
                if (messageDocument is not null)
                {
                    var messageDocumentFileName = Path.GetFileNameWithoutExtension(messageDocument.FileName);
                    var extension = Path.GetExtension(messageDocument.FileName);

                    outboxMessage.Document = messageDocument;
                    using var dataStream = new MemoryStream();
                    await outboxMessage.Document.CopyToAsync(dataStream);
                    outboxMessage.Attachment = dataStream.ToArray();
                    outboxMessage.FileType = messageDocument.ContentType;
                    outboxMessage.Extension = extension;
                    outboxMessage.AttachmentName = messageDocumentFileName;
                }

                userMailbox.Outbox.Add(outboxMessage);
                _context.Mailbox.Update(userMailbox);
                var rows = await _context.SaveChangesAsync();
            }
            return false;
        }
    }
}
