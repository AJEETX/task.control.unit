using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Models;

namespace risk.control.system.Controllers.Api
{
    public class DownloadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DownloadController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> InboxDetailsDownloadFileAttachment(int id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }

            var userMailbox = _context.Mailbox.Include(m => m.Inbox)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            var InboxFile = userMailbox.Inbox.FirstOrDefault(c => c.InboxMessageId == id);

            return InboxFile != null ? File(InboxFile.Attachment, InboxFile.FileType, InboxFile.AttachmentName + InboxFile.Extension) : Problem();
        }
        public async Task<IActionResult> OutboxDetailsDownloadFileAttachment(int id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }

            var userMailbox = _context.Mailbox.Include(m => m.Outbox)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            OutboxMessage? outBox = userMailbox.Outbox.FirstOrDefault(c => c.OutboxMessageId == id);

            return outBox != null ? File(outBox.Attachment, outBox.FileType, outBox.AttachmentName + outBox.Extension) : Problem();
        }
        public async Task<IActionResult> SentDetailsDownloadFileAttachment(int id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }

            var userMailbox = _context.Mailbox.Include(m => m.Sent)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            SentMessage? sentBox = userMailbox.Sent.FirstOrDefault(c => c.SentMessageId == id);

            return sentBox != null ? File(sentBox.Attachment, sentBox.FileType, sentBox.AttachmentName + sentBox.Extension) : Problem();
        }
        public async Task<IActionResult> TrashDetailsDownloadFileAttachment(int id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }

            var userMailbox = _context.Mailbox.Include(m => m.Trash)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            TrashMessage? trash = userMailbox.Trash.FirstOrDefault(c => c.TrashMessageId == id);

            return trash != null ? File(trash.Attachment, trash.FileType, trash.AttachmentName + trash.Extension) : Problem();
        }
    }
}
