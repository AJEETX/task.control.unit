using System.Text.Json.Serialization;
using System.Text.Json;

using risk.control.system.Models;
using risk.control.system.Data;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace risk.control.system.Services
{
    public interface ISentMailService
    {
        Task<IEnumerable<SentMessage>> GetSentMessages(string userEmail);
        Task<int> SentDelete(List<long> messages, long userId);
        Task<SentMessage> GetSentMessagedetail(long messageId, string userEmail);
        Task<SentMessage> GetSentMessagedetailReply(long messageId, string userEmail, string actiontype);
        Task<bool> SendReplyMessage(SentMessage contactMessage, string userEmail, IFormFile? messageDocument);

    }
    public class SentMailService : ISentMailService
    {
        private readonly JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
        private readonly ApplicationDbContext _context;

        public SentMailService(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<SentMessage> GetSentMessagedetailReply(long messageId, string userEmail, string actiontype)
        {
            var userMailbox = _context.Mailbox
             .Include(m => m.Sent)
             .FirstOrDefault(c => c.Name == userEmail);

            var userMessage = userMailbox.Sent.FirstOrDefault(c => c.SentMessageId == messageId);

            var replyRawMessage = "<br />" + "<hr />" + "From: " + userMessage.SenderEmail + "<br />" + "<hr />" + "Sent:" + userMessage.SendDate + "<br />" + "<hr />" + userMessage.RawMessage;

            var userReplyMessage = new SentMessage
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
        public async Task<SentMessage> GetSentMessagedetail(long messageId, string userEmail)
        {
            var userMailbox = _context.Mailbox
                .Include(m => m.Sent)
                .FirstOrDefault(c => c.Name == userEmail);

            var userMessage = userMailbox.Sent.FirstOrDefault(c => c.SentMessageId == messageId);
            userMessage.Read = true;
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return userMessage ;
        }

        public async Task<IEnumerable<SentMessage>> GetSentMessages(string userEmail)
        {
            var userMailbox = _context.Mailbox.Include(m => m.Sent).FirstOrDefault(c => c.Name == userEmail);
            return userMailbox.Sent.OrderByDescending(o => o.SendDate).ToList();
        }

        public async Task<int> SentDelete(List<long> messages, long userId)
        {
            var userMailbox = _context.Mailbox
                           .Include(m => m.Sent)
                           .Include(m => m.Trash)
                           .FirstOrDefault(c => c.ApplicationUserId == userId);

            var userSentMails = userMailbox.Sent.Where(d => messages.Contains(d.SentMessageId)).ToList();

            if (userSentMails is not null && userSentMails.Count > 0)
            {
                foreach (var message in userSentMails)
                {
                    message.MessageStatus = MessageStatus.TRASHED;
                    userMailbox.Sent.Remove(message);
                    var jsonMessage = JsonSerializer.Serialize(message, options);
                    TrashMessage trashMessage = JsonSerializer.Deserialize<TrashMessage>(jsonMessage, options);
                    userMailbox.Trash.Add(trashMessage);
                }
            }
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return rows;
        }

        public async Task<bool> SendReplyMessage(SentMessage contactMessage, string userEmail, IFormFile? messageDocument)
        {
            contactMessage.Message = HttpUtility.HtmlEncode(contactMessage.RawMessage);

            var userMailbox = _context.Mailbox.FirstOrDefault(c => c.Name == userEmail);

            var recepientMailbox = _context.Mailbox.FirstOrDefault(c => c.Name == contactMessage.ReceipientEmail);
            contactMessage.SenderEmail = userEmail;
            contactMessage.SendDate = DateTime.Now;
            contactMessage.Read = false;

            if (recepientMailbox is not null)
            {
                contactMessage.MessageStatus = MessageStatus.SENT;
                var jsonMessage = JsonSerializer.Serialize(contactMessage, options);
                SentMessage sentMessage = JsonSerializer.Deserialize<SentMessage>(jsonMessage, options);

                if (messageDocument is not null)
                {
                    var messageDocumentFileName = Path.GetFileNameWithoutExtension(messageDocument.FileName);
                    var extension = Path.GetExtension(messageDocument.FileName);

                    sentMessage.Document = (FormFile?)messageDocument;
                    using var dataStream = new MemoryStream();
                    await sentMessage.Document.CopyToAsync(dataStream);
                    sentMessage.Attachment = dataStream.ToArray();
                    sentMessage.FileType = messageDocument.ContentType;
                    sentMessage.Extension = extension;
                    sentMessage.AttachmentName = messageDocumentFileName;
                }
                sentMessage.SendDate = DateTime.Now;
                userMailbox.Sent.Add(sentMessage);
                _context.Mailbox.Attach(userMailbox);
                _context.Mailbox.Update(userMailbox);
                InboxMessage inboxMessage = JsonSerializer.Deserialize<InboxMessage>(jsonMessage, options);

                if (messageDocument is not null)
                {
                    var messageDocumentFileName = Path.GetFileNameWithoutExtension(messageDocument.FileName);
                    var extension = Path.GetExtension(messageDocument.FileName);
                    inboxMessage.Document = (FormFile?)messageDocument;
                    using var dataStream = new MemoryStream();
                    await inboxMessage.Document.CopyToAsync(dataStream);
                    inboxMessage.Attachment = dataStream.ToArray();
                    inboxMessage.FileType = messageDocument.ContentType;
                    inboxMessage.Extension = extension;
                    inboxMessage.AttachmentName = messageDocumentFileName;
                }
                inboxMessage.SendDate = DateTime.Now;
                recepientMailbox.Inbox.Add(inboxMessage);
                _context.Mailbox.Attach(recepientMailbox);
                _context.Mailbox.Update(recepientMailbox);
                var rowse = await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                if (messageDocument is not null)
                {
                    var messageDocumentFileName = Path.GetFileNameWithoutExtension(messageDocument.FileName);
                    var extension = Path.GetExtension(messageDocument.FileName);

                    contactMessage.Document = (FormFile?)messageDocument;
                    using var dataStream = new MemoryStream();
                    await contactMessage.Document.CopyToAsync(dataStream);
                    contactMessage.Attachment = dataStream.ToArray();
                    contactMessage.FileType = messageDocument.ContentType;
                    contactMessage.Extension = extension;
                    contactMessage.AttachmentName = messageDocumentFileName;
                }

                var jsonMessage = JsonSerializer.Serialize(contactMessage, options);
                OutboxMessage outboxMessage = JsonSerializer.Deserialize<OutboxMessage>(jsonMessage, options);
                userMailbox.Outbox.Add(outboxMessage);
                _context.Mailbox.Update(userMailbox);
                var rowse = await _context.SaveChangesAsync();
                return false;
            }
        }
    }
}
