using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Mailbox")]
    public class ContactMessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISentMailService sentMailService;
        private readonly IInboxMailService inboxMailService;
        private readonly ITrashMailService trashMailService;
        private readonly IToastNotification toastNotification;

        private readonly JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        public ContactMessageController(ApplicationDbContext context, ISentMailService sentMailService,
            IInboxMailService inboxMailService,
            ITrashMailService trashMailService,
            IToastNotification toastNotification)
        {
            _context = context;
            this.sentMailService = sentMailService;
            this.inboxMailService = inboxMailService;
            this.trashMailService = trashMailService;
            this.toastNotification = toastNotification;
        }

        // GET: ContactMessage
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailboxMessages = await inboxMailService.GetInboxMessages(userEmail);

            return View(userMailboxMessages.OrderByDescending(o => o.SendDate));
        }

        [Breadcrumb("Inbox", FromAction = "Index")]
        public async Task<IActionResult> Inbox()
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailboxMessages = await inboxMailService.GetInboxMessages(userEmail);

            return View("Index", userMailboxMessages.OrderByDescending(o => o.SendDate));
        }

        [Breadcrumb("Delete", FromAction = "Inbox")]
        public async Task<IActionResult> InboxDelete(List<long> messages)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var rows = await inboxMailService.InboxDelete(messages, applicationUser.Id);
            toastNotification.AddSuccessToastMessage($" {messages.Count} mail(s) trashed successfully!");

            return RedirectToAction(nameof(Index));
        }

        [Breadcrumb("Details", FromAction = "Inbox")]
        public async Task<IActionResult> InboxDetails(long id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMessage = await inboxMailService.GetInboxMessagedetail(id, userEmail);
            return View(userMessage);
        }

        [Breadcrumb("Reply", FromAction = "Inbox")]
        public async Task<IActionResult> InboxDetailsReply(long id, string actiontype)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMessage = await inboxMailService.GetInboxMessagedetailReply(id, userEmail, actiontype);
            ViewBag.ActionType = actiontype;
            return View(userMessage);
        }

        [HttpPost]
        public async Task<IActionResult> InboxDetailsReply(OutboxMessage contactMessage)
        {
            contactMessage.Message = HttpUtility.HtmlEncode(contactMessage.RawMessage);

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }

            IFormFile? messageDocument = Request.Form?.Files?.FirstOrDefault();

            var mailSent = await inboxMailService.SendReplyMessage(contactMessage, userEmail, messageDocument);

            if (mailSent)
            {
                toastNotification.AddSuccessToastMessage("mail sent successfully!");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                toastNotification.AddErrorToastMessage("Error: recepient email incorrect!");
                return RedirectToAction(nameof(Create));
            }
        }

        [Breadcrumb("Delete", FromAction = "InboxDetails")]
        public async Task<IActionResult> InboxDetailsDelete(long id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var rows = await inboxMailService.InboxDetailsDelete(id, userEmail);
            toastNotification.AddSuccessToastMessage($"mail trashed successfully!");

            return RedirectToAction(nameof(Index));
        }

        [Breadcrumb("Trash", FromAction = "Index")]
        public async Task<IActionResult> Trash()
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var usertrashMessages = await trashMailService.GetTrashMessages(userEmail);

            return View(usertrashMessages.OrderByDescending(o => o.SendDate).ToList());
        }

        [Breadcrumb("Delete", FromAction = "Trash")]
        public async Task<IActionResult> TrashDelete(List<long> messages)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var rows = await trashMailService.TrashDelete(messages, applicationUser.Id);

            toastNotification.AddSuccessToastMessage($" {messages.Count} mail(s) deleted permanently successfully!");

            return RedirectToAction(nameof(Trash));
        }

        [Breadcrumb("Details", FromAction = "Trash")]
        public async Task<IActionResult> TrashDetails(long id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMessage = await trashMailService.GetTrashMessagedetail(id, userEmail);
            return View(userMessage);
        }

        [Breadcrumb("Delete", FromAction = "TrashDetails")]
        public async Task<IActionResult> TrashDetailsDelete(long id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var rows = await trashMailService.TrashDetailsDelete(id, userEmail);
            toastNotification.AddSuccessToastMessage($" {rows} mail deleted permanently successfully!");

            return RedirectToAction(nameof(Trash));
        }

        [Breadcrumb("Sent", FromAction = "Index")]
        public async Task<IActionResult> Sent()
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailboxMessages = await sentMailService.GetSentMessages(userEmail);

            return View(userMailboxMessages.OrderByDescending(o => o.SendDate));
        }

        [Breadcrumb("Delete", FromAction = "Sent")]
        public async Task<IActionResult> SentDelete(List<long> messages)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var rows = await sentMailService.SentDelete(messages, applicationUser.Id);
            toastNotification.AddSuccessToastMessage($" {rows} mail(s) trashed successfully!");

            return RedirectToAction(nameof(Sent));
        }

        [Breadcrumb("Details", FromAction = "Sent")]
        public async Task<IActionResult> SentDetails(long id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMessage = await sentMailService.GetSentMessagedetail(id, userEmail);
            return View(userMessage);
        }

        [Breadcrumb("Reply", FromAction = "SentDetails")]
        public async Task<IActionResult> SentDetailsReply(long id, string actiontype)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMessage = await sentMailService.GetSentMessagedetailReply(id, userEmail, actiontype);

            ViewBag.ActionType = actiontype;
            return View(userMessage);
        }

        [HttpPost]
        public async Task<IActionResult> SentDetailsReply(SentMessage contactMessage)
        {
            contactMessage.Message = HttpUtility.HtmlEncode(contactMessage.RawMessage);

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            IFormFile? messageDocument = Request.Form?.Files?.FirstOrDefault();

            var mailSent = await sentMailService.SendReplyMessage(contactMessage, userEmail, messageDocument);

            if (mailSent)
            {
                toastNotification.AddSuccessToastMessage("mail sent successfully!");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                toastNotification.AddErrorToastMessage("Error: recepient email incorrect!");
                return RedirectToAction(nameof(Create));
            }
        }

        [Breadcrumb("Delete", FromAction = "SentDetails")]
        public async Task<IActionResult> SentDetailsDelete(long id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailbox = _context.Mailbox
                .Include(m => m.Sent)
                .Include(m => m.Trash)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            var userSentMessage = userMailbox.Sent.FirstOrDefault(c => c.SentMessageId == id);

            if (userSentMessage is not null)
            {
                userSentMessage.MessageStatus = MessageStatus.TRASHED;
                userMailbox.Sent.Remove(userSentMessage);
                var jsonMessage = JsonSerializer.Serialize(userSentMessage, options);
                TrashMessage trashMessage = JsonSerializer.Deserialize<TrashMessage>(jsonMessage, options);
                userMailbox.Trash.Add(trashMessage);
            }

            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage($" {rows} mail trashed successfully!");

            return RedirectToAction(nameof(Sent));
        }

        [Breadcrumb("Outbox", FromAction = "Index")]
        public async Task<IActionResult> Outbox()
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailbox = _context.Mailbox.Include(m => m.Outbox).FirstOrDefault(c => c.Name == applicationUser.Email);

            return View(userMailbox.Outbox.OrderByDescending(o => o.SendDate).ToList());
        }

        [Breadcrumb("Delete", FromAction = "Outbox")]
        public async Task<IActionResult> OutboxDelete(List<long> messages)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailbox = _context.Mailbox
                           .Include(m => m.Outbox)
                           .Include(m => m.Trash)
                           .FirstOrDefault(c => c.ApplicationUserId == applicationUser.Id);

            var userOutboxMails = userMailbox.Outbox.Where(d => messages.Contains(d.OutboxMessageId)).ToList();

            if (userOutboxMails is not null && userOutboxMails.Count > 0)
            {
                foreach (var message in userOutboxMails)
                {
                    message.MessageStatus = MessageStatus.TRASHED;
                    userMailbox.Outbox.Remove(message);
                    var jsonMessage = JsonSerializer.Serialize(message, options);
                    TrashMessage trashedMessage = JsonSerializer.Deserialize<TrashMessage>(jsonMessage, options);
                    userMailbox.Trash.Add(trashedMessage);
                }
            }
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage($" {rows} mail(s) trashed successfully!");

            return RedirectToAction(nameof(Outbox));
        }

        // GET: ContactMessage/Details/5
        [Breadcrumb("Details", FromAction = "Outbox")]
        public async Task<IActionResult> OutBoxDetails(long id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailbox = _context.Mailbox
                .Include(m => m.Outbox)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            var userMessage = userMailbox.Outbox.FirstOrDefault(c => c.OutboxMessageId == id);
            userMessage.Read = true;
            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            return View(userMessage);
        }

        [Breadcrumb("Delete", FromAction = "OutBoxDetails")]
        public async Task<IActionResult> OutboxDetailsDelete(long id)
        {
            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }
            var userMailbox = _context.Mailbox
                .Include(m => m.Outbox)
                .FirstOrDefault(c => c.Name == applicationUser.Email);

            var userOutboxMessage = userMailbox.Outbox.FirstOrDefault(c => c.OutboxMessageId == id);

            if (userOutboxMessage is not null)
            {
                userOutboxMessage.MessageStatus = MessageStatus.TRASHED;
                userMailbox.Outbox.Remove(userOutboxMessage);
                var jsonMessage = JsonSerializer.Serialize(userOutboxMessage, options);
                TrashMessage trashMessage = JsonSerializer.Deserialize<TrashMessage>(jsonMessage, options);
                userMailbox.Trash.Add(trashMessage);
            }

            _context.Mailbox.Update(userMailbox);
            var rows = await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage($" {rows} mail trashed successfully!");

            return RedirectToAction(nameof(Outbox));
        }

        // GET: ContactMessage/Create
        [Breadcrumb("Compose", FromAction = "Index")]
        public IActionResult Create()
        {
            ViewData["ApplicationUserId"] = new SelectList(_context.ApplicationUser, "Id", "CountryId");
            return View();
        }

        // POST: ContactMessage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OutboxMessage contactMessage)
        {
            contactMessage.Message = HttpUtility.HtmlEncode(contactMessage.RawMessage);

            var userEmail = HttpContext.User.Identity.Name;

            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            if (applicationUser == null)
            {
                return NotFound();
            }

            IFormFile? messageDocument = Request.Form?.Files?.FirstOrDefault();

            var mailSent = await inboxMailService.SendReplyMessage(contactMessage, userEmail, messageDocument);

            if (mailSent)
            {
                toastNotification.AddSuccessToastMessage("mail sent successfully!");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                toastNotification.AddErrorToastMessage("Error: recepient email incorrect!");
                return RedirectToAction(nameof(Create));
            }
        }
    }
}