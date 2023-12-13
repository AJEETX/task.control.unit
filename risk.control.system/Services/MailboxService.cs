using System.Net.Http.Headers;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace risk.control.system.Services
{
    public interface IMailboxService
    {
        Task NotifyClaimCreation(string userEmail, ClaimsInvestigation claimsInvestigation);

        Task NotifyClaimAllocationToVendor(string userEmail, string policy, string claimsInvestigationId, string vendorId, long caseLocationId);

        Task NotifyClaimAssignmentToAssigner(string userEmail, List<string> claims);

        Task NotifyClaimAssignmentToVendorAgent(string senderUserEmail, string claimId, string agentEmail, string vendorId, long caseLocationId);

        Task NotifyClaimReportSubmitToVendorSupervisor(string senderUserEmail, string claimId, long caseLocationId);

        Task NotifyClaimReportSubmitToCompany(string senderUserEmail, string claimId, long caseLocationId);

        Task NotifyClaimReportProcess(string senderUserEmail, string claimId, long caseLocationId);
    }

    public class MailboxService : IMailboxService
    {
        private static string BaseUrl = string.Empty;
        private static string AgencyBaseUrl = string.Empty;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;
        private readonly UserManager<VendorApplicationUser> userVendorManager;
        private static HttpClient client = new HttpClient();

        public MailboxService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ClientCompanyApplicationUser> userManager, UserManager<VendorApplicationUser> userVendorManager)
        {
            this._context = context;
            this.httpContextAccessor = httpContextAccessor;
            var host = httpContextAccessor?.HttpContext?.Request.Host.ToUriComponent();
            var pathBase = httpContextAccessor?.HttpContext?.Request.PathBase.ToUriComponent();

            BaseUrl = $"{httpContextAccessor?.HttpContext?.Request.Scheme}://{host}{pathBase}/ClaimsInvestigation/Detail/";
            AgencyBaseUrl = $"{httpContextAccessor?.HttpContext?.Request.Scheme}://{host}{pathBase}/ClaimsVendor/Detail/";
            this.userManager = userManager;
            this.userVendorManager = userVendorManager;
        }

        public async Task NotifyClaimAllocationToVendor(string userEmail, string policy, string claimsInvestigationId, string vendorId, long caseLocationId)
        {
            //1. get vendor admin and supervisor email

            var supervisorRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Supervisor.ToString()));
            var agencyAdminRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.AgencyAdmin.ToString()));

            var vendorUsers = _context.VendorApplicationUser.Where(u => u.VendorId == vendorId);

            List<VendorApplicationUser> userEmailsToSend = new();

            foreach (var assignedUser in vendorUsers)
            {
                var isAdmin = await userVendorManager.IsInRoleAsync(assignedUser, agencyAdminRole?.Name);
                if (isAdmin)
                {
                    userEmailsToSend.Add(assignedUser);
                }
                var isSupervisor = await userVendorManager.IsInRoleAsync(assignedUser, supervisorRole?.Name);
                if (isSupervisor)
                {
                    userEmailsToSend.Add(assignedUser);
                }
            }

            string claimsUrl = $"{AgencyBaseUrl + claimsInvestigationId}";

            var claimsInvestigation = _context.ClaimsInvestigation
                .Include(i => i.PolicyDetail)
                .Include(i => i.InvestigationCaseSubStatus)
                .FirstOrDefault(v => v.ClaimsInvestigationId == claimsInvestigationId);

            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            var clientCompanyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == clientCompanyUser.ClientCompanyId);

            foreach (var userEmailToSend in userEmailsToSend)
            {
                var recepientMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == userEmailToSend.Email);
                var contactMessage = new InboxMessage
                {
                    //ReceipientEmail = userEmailToSend,
                    Created = DateTime.UtcNow,
                    Message = "Claim(s) allocated ",
                    Subject = "Claim(s) allocated: Policy #" + policy + " ",
                    SenderEmail = userEmail,
                    Priority = ContactMessagePriority.URGENT,
                    SendDate = DateTime.Now,
                    Updated = DateTime.Now,
                    Read = false,
                    UpdatedBy = userEmail,
                    ReceipientEmail = recepientMailbox.Name,
                    RawMessage = MailText
                        .Replace("[username]", recepientMailbox.Name)
                        .Replace("[email]", recepientMailbox.Name)
                        .Replace("[url]", claimsUrl)
                        .Replace("[stage]", claimsInvestigation.InvestigationCaseSubStatus.Name)
                        .Replace("[policy]", claimsInvestigation.PolicyDetail.ContractNumber)
                        .Replace("[logo]",
                        claimsInvestigation.PolicyDetail?.DocumentImage != null ?
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(claimsInvestigation.PolicyDetail?.DocumentImage))
                        : "/img/no-image.png")
                };
                recepientMailbox?.Inbox.Add(contactMessage);
                _context.Mailbox.Attach(recepientMailbox);
                _context.Mailbox.Update(recepientMailbox);
                //SEND SMS
                if (company.SendSMS)
                {
                    var result = SmsService.SendSingleMessage(userEmailToSend.PhoneNumber, "Claim(s) allocated (" + userEmailToSend.Email + "): Policy #" + policy + " ", true);
                }
                //SMS ::END
            }

            try
            {
                var rows = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task NotifyClaimAssignmentToAssigner(string senderUserEmail, List<string> claims)
        {
            var applicationUser = _context.ApplicationUser.Where(u => u.Email == senderUserEmail).FirstOrDefault();
            List<ClientCompanyApplicationUser> userEmailsToSend = new List<ClientCompanyApplicationUser>();

            var clientCompanyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == applicationUser.Email);

            var assignerRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Assigner.ToString()));

            var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == clientCompanyUser.ClientCompanyId);

            foreach (var companyUser in companyUsers)
            {
                var isAssigner = await userManager.IsInRoleAsync(companyUser, assignerRole?.Name);
                if (isAssigner)
                {
                    userEmailsToSend.Add(companyUser);
                }
            }

            var claimsInvestigations = _context.ClaimsInvestigation
                .Include(i => i.PolicyDetail)
                .Include(i => i.InvestigationCaseSubStatus)
                .Where(v => claims.Contains(v.ClaimsInvestigationId));

            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == clientCompanyUser.ClientCompanyId);

            foreach (var userEmailToSend in userEmailsToSend)
            {
                var recepientMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == userEmailToSend.Email);
                var contactMessage = new InboxMessage
                {
                    //ReceipientEmail = userEmailToSend,
                    Message = "Claim(s) assigned ",
                    Created = DateTime.UtcNow,
                    Subject = "Claim(s) assigned:",
                    SenderEmail = clientCompanyUser?.Email ?? applicationUser.Email,
                    Priority = ContactMessagePriority.NORMAL,
                    SendDate = DateTime.Now,
                    Updated = DateTime.Now,
                    Read = false,
                    UpdatedBy = applicationUser.Email,
                    ReceipientEmail = recepientMailbox.Name
                };

                foreach (var claimsInvestigation in claimsInvestigations)
                {
                    string claimsUrl = $"{BaseUrl + claimsInvestigation.ClaimsInvestigationId} ";
                    contactMessage.Subject += claimsInvestigation.PolicyDetail.ContractNumber + ", ";
                    contactMessage.RawMessage += MailText
                        .Replace("[username]", recepientMailbox.Name)
                        .Replace("[email]", recepientMailbox.Name)
                        .Replace("[url]", claimsUrl)
                        .Replace("[stage]", claimsInvestigation.InvestigationCaseSubStatus.Name)
                        .Replace("[policy]", claimsInvestigation.PolicyDetail.ContractNumber)
                        .Replace("[logo]",
                        claimsInvestigation.PolicyDetail?.DocumentImage != null ?
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(claimsInvestigation.PolicyDetail?.DocumentImage))
                        : "/img/no-image.png")
                        ;
                }
                recepientMailbox?.Inbox.Add(contactMessage);
                _context.Mailbox.Attach(recepientMailbox);
                _context.Mailbox.Update(recepientMailbox);
                //SEND SMS
                if (company.SendSMS)
                {
                    var result = SmsService.SendSingleMessage(userEmailToSend.PhoneNumber, "Claim(s) allocated: Policy #" + claimsInvestigations.Count() + " ");
                }
                //SMS ::END
            }
            try
            {
                var rows = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task NotifyClaimAssignmentToVendorAgent(string userEmail, string claimId, string agentEmail, string vendorId, long caseLocationId)
        {
            var agentRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Agent.ToString()));

            var recepientMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == agentEmail);
            var recepientUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == agentEmail);

            string claimsUrl = $"{AgencyBaseUrl + claimId}";

            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            var claimsInvestigation = _context.ClaimsInvestigation
                .Include(i => i.PolicyDetail)
                .Include(i => i.InvestigationCaseSubStatus)
                .FirstOrDefault(v => v.ClaimsInvestigationId == claimId);
            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claimsInvestigation.PolicyDetail.ClientCompanyId);

            var contactMessage = new InboxMessage
            {
                //ReceipientEmail = userEmailToSend,
                Message = "Claim tasked ",
                Created = DateTime.UtcNow,
                Subject = "Claim tasked [Policy #:" + claimsInvestigation.PolicyDetail.ContractNumber,
                SenderEmail = userEmail,
                Priority = ContactMessagePriority.URGENT,
                SendDate = DateTime.Now,
                Updated = DateTime.Now,
                Read = false,
                UpdatedBy = userEmail,
                ReceipientEmail = recepientMailbox.Name,
                RawMessage = MailText
                        .Replace("[username]", recepientMailbox.Name)
                        .Replace("[email]", recepientMailbox.Name)
                        .Replace("[url]", claimsUrl)
                        .Replace("[stage]", claimsInvestigation.InvestigationCaseSubStatus.Name)
                        .Replace("[policy]", claimsInvestigation.PolicyDetail.ContractNumber)
                        .Replace("[logo]",
                        claimsInvestigation.PolicyDetail?.DocumentImage != null ?
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(claimsInvestigation.PolicyDetail?.DocumentImage))
                        : "/img/no-image.png")
            };
            recepientMailbox?.Inbox.Add(contactMessage);
            _context.Mailbox.Attach(recepientMailbox);
            _context.Mailbox.Update(recepientMailbox);
            try
            {
                var rows = await _context.SaveChangesAsync();
                if (company.SendSMS)
                {
                    var result = SmsService.SendSingleMessage(recepientUser.PhoneNumber, "Claim(s) allocated: Policy #" + claimsInvestigation.PolicyDetail.ContractNumber + " ");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task NotifyClaimCreation(string userEmail, ClaimsInvestigation claimsInvestigation)
        {
            var applicationUser = _context.ApplicationUser.Where(u => u.Email == userEmail).FirstOrDefault();
            List<string> userEmailsToSend = new();

            var clientCompanyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == applicationUser.Email);
            if (clientCompanyUser == null)
            {
                userEmailsToSend.Add(userEmail);
            }
            else
            {
                var creatorRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Creator.ToString()));

                var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == clientCompanyUser.ClientCompanyId);

                foreach (var companyUser in companyUsers)
                {
                    var userInCreatorRole = await userManager.IsInRoleAsync(companyUser, creatorRole?.Name);
                    if (userInCreatorRole)
                    {
                        userEmailsToSend.Add(companyUser.Email);
                    }
                }
            }
            string claimsUrl = $"<a href={BaseUrl + claimsInvestigation.ClaimsInvestigationId}>url</a>";
            claimsUrl = "<html>" + Environment.NewLine + claimsUrl + Environment.NewLine + "</html>";

            var contactMessage = new InboxMessage
            {
                //ReceipientEmail = userEmailToSend,
                Message = "Claim created ",
                Created = DateTime.UtcNow,
                RawMessage = claimsUrl,
                Subject = $"Claim created: case Id = {claimsInvestigation.ClaimsInvestigationId}",
                SenderEmail = clientCompanyUser?.Email ?? applicationUser.Email,
                Priority = ContactMessagePriority.NORMAL,
                SendDate = DateTime.Now,
                Updated = DateTime.Now,
                Read = false,
                UpdatedBy = applicationUser.Email
            };
            if (claimsInvestigation.PolicyDetail.Document is not null)
            {
                var messageDocumentFileName = Path.GetFileNameWithoutExtension(claimsInvestigation.PolicyDetail.Document.FileName);
                var extension = Path.GetExtension(claimsInvestigation.PolicyDetail.Document.FileName);
                contactMessage.Document = claimsInvestigation.PolicyDetail.Document;
                using var dataStream = new MemoryStream();
                await contactMessage.Document.CopyToAsync(dataStream);
                contactMessage.Attachment = dataStream.ToArray();
                contactMessage.FileType = claimsInvestigation.PolicyDetail.Document.ContentType;
                contactMessage.Extension = extension;
                contactMessage.AttachmentName = messageDocumentFileName;
            }

            foreach (var userEmailToSend in userEmailsToSend)
            {
                var recepientMailbox = _context.Mailbox.FirstOrDefault(c => c.Name == userEmailToSend);
                contactMessage.ReceipientEmail = recepientMailbox.Name;
                recepientMailbox?.Inbox.Add(contactMessage);
                _context.Mailbox.Attach(recepientMailbox);
                _context.Mailbox.Update(recepientMailbox);
            }
            try
            {
                var rows = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task NotifyClaimReportProcess(string senderUserEmail, string claimId, long caseLocationId)
        {
            var claim = _context.ClaimsInvestigation.Include(p => p.PolicyDetail).Where(c => c.ClaimsInvestigationId == claimId).FirstOrDefault();
            if (claim != null)
            {
                var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == claim.PolicyDetail.ClientCompanyId);

                var clientAdminrRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.CompanyAdmin.ToString()));

                List<ClientCompanyApplicationUser> users = new List<ClientCompanyApplicationUser>();
                foreach (var user in companyUsers)
                {
                    var isAdmin = await userManager.IsInRoleAsync(user, clientAdminrRole?.Name);
                    if (isAdmin)
                    {
                        users.Add(user);
                    }
                }

                string claimsUrl = $"{BaseUrl + claimId}";

                string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();

                var claimsInvestigation = _context.ClaimsInvestigation
                    .Include(i => i.PolicyDetail)
                    .Include(i => i.InvestigationCaseSubStatus)
                    .FirstOrDefault(v => v.ClaimsInvestigationId == claimId);

                var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claimsInvestigation.PolicyDetail.ClientCompanyId);

                foreach (var user in users)
                {
                    var recepientMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == user.Email);
                    var contactMessage = new InboxMessage
                    {
                        //ReceipientEmail = userEmailToSend,
                        Message = "Claim process ",
                        Created = DateTime.UtcNow,
                        Subject = "Claim Policy #:" + claimsInvestigation.PolicyDetail.ContractNumber,
                        SenderEmail = senderUserEmail,
                        Priority = ContactMessagePriority.NORMAL,
                        SendDate = DateTime.Now,
                        Updated = DateTime.Now,
                        Read = false,
                        UpdatedBy = senderUserEmail,
                        ReceipientEmail = recepientMailbox.Name,
                        RawMessage = MailText
                        .Replace("[username]", recepientMailbox.Name)
                        .Replace("[email]", recepientMailbox.Name)
                        .Replace("[url]", claimsUrl)
                        .Replace("[stage]", claimsInvestigation.InvestigationCaseSubStatus.Name)
                        .Replace("[policy]", claimsInvestigation.PolicyDetail.ContractNumber)
                        .Replace("[logo]",
                        claimsInvestigation.PolicyDetail?.DocumentImage != null ?
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(claimsInvestigation.PolicyDetail?.DocumentImage))
                        : "/img/no-image.png")
                    };
                    recepientMailbox?.Inbox.Add(contactMessage);
                    _context.Mailbox.Attach(recepientMailbox);
                    _context.Mailbox.Update(recepientMailbox);
                    if (company.SendSMS)
                    {
                        var result = SmsService.SendSingleMessage(user.PhoneNumber, "Claim(s) allocated: Policy #" + claimsInvestigation.PolicyDetail.ContractNumber + " ");
                    }
                }
                try
                {
                    var rows = await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task NotifyClaimReportSubmitToCompany(string senderUserEmail, string claimId, long caseLocationId)
        {
            var claim = _context.ClaimsInvestigation.Include(i => i.PolicyDetail).Where(c => c.ClaimsInvestigationId == claimId).FirstOrDefault();
            if (claim != null)
            {
                var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == claim.PolicyDetail.ClientCompanyId);

                var assessorRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Assessor.ToString()));

                List<ClientCompanyApplicationUser> users = new List<ClientCompanyApplicationUser>();
                foreach (var user in companyUsers)
                {
                    var isAssessor = await userManager.IsInRoleAsync(user, assessorRole?.Name);
                    if (isAssessor)
                    {
                        users.Add(user);
                    }
                }

                string claimsUrl = $"{BaseUrl + claimId}";

                string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();

                var claimsInvestigation = _context.ClaimsInvestigation
                    .Include(i => i.PolicyDetail)
                    .Include(i => i.InvestigationCaseSubStatus)
                    .FirstOrDefault(v => v.ClaimsInvestigationId == claimId);
                var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claimsInvestigation.PolicyDetail.ClientCompanyId);

                foreach (var user in users)
                {
                    var recepientMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == user.Email);
                    var contactMessage = new InboxMessage
                    {
                        //ReceipientEmail = userEmailToSend,
                        Message = "Claim report ",
                        Created = DateTime.UtcNow,
                        Subject = "Claim report Policy #:" + claimsInvestigation.PolicyDetail.ContractNumber,
                        SenderEmail = senderUserEmail,
                        Priority = ContactMessagePriority.NORMAL,
                        SendDate = DateTime.Now,
                        Updated = DateTime.Now,
                        Read = false,
                        UpdatedBy = senderUserEmail,
                        ReceipientEmail = recepientMailbox.Name,
                        RawMessage = MailText
                        .Replace("[username]", recepientMailbox.Name)
                        .Replace("[email]", recepientMailbox.Name)
                        .Replace("[url]", claimsUrl)
                        .Replace("[stage]", claimsInvestigation.InvestigationCaseSubStatus.Name)
                        .Replace("[policy]", claimsInvestigation.PolicyDetail.ContractNumber)
                        .Replace("[logo]",
                        claimsInvestigation.PolicyDetail?.DocumentImage != null ?
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(claimsInvestigation.PolicyDetail?.DocumentImage))
                        : "/img/no-image.png")
                    };
                    recepientMailbox?.Inbox.Add(contactMessage);
                    _context.Mailbox.Attach(recepientMailbox);
                    _context.Mailbox.Update(recepientMailbox);
                    if (company.SendSMS)
                    {
                        var result = SmsService.SendSingleMessage(user.PhoneNumber, "Claim(s) allocated: Policy #" + claimsInvestigation.PolicyDetail.ContractNumber + " ");
                    }
                }
                try
                {
                    var rows = await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task NotifyClaimReportSubmitToVendorSupervisor(string senderUserEmail, string claimId, long caseLocationId)
        {
            var supervisorRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Supervisor.ToString()));

            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(u => u.Email == senderUserEmail);

            var vendorUsers = _context.VendorApplicationUser.Where(u => u.VendorId == vendorUser.VendorId);

            List<VendorApplicationUser> users = new List<VendorApplicationUser>();

            foreach (var user in vendorUsers)
            {
                var isSupervisor = await userVendorManager.IsInRoleAsync(user, supervisorRole?.Name);
                if (isSupervisor)
                {
                    users.Add(user);
                }
            }
            string claimsUrl = $"{AgencyBaseUrl + claimId}";
            string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\WelcomeTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            var claimsInvestigation = _context.ClaimsInvestigation
                .Include(i => i.PolicyDetail)
                .Include(i => i.InvestigationCaseSubStatus)
                .FirstOrDefault(v => v.ClaimsInvestigationId == claimId);
            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claimsInvestigation.PolicyDetail.ClientCompanyId);

            foreach (var user in users)
            {
                var recepientMailbox = _context.Mailbox.Include(m => m.Inbox).FirstOrDefault(c => c.Name == user.Email);
                var contactMessage = new InboxMessage
                {
                    //ReceipientEmail = userEmailToSend,
                    Message = "Claim report ",
                    Created = DateTime.UtcNow,
                    Subject = "New Claim report Policy #:" + claimsInvestigation.PolicyDetail.ContractNumber,
                    SenderEmail = senderUserEmail,
                    Priority = ContactMessagePriority.NORMAL,
                    SendDate = DateTime.Now,
                    Updated = DateTime.Now,
                    Read = false,
                    UpdatedBy = senderUserEmail,
                    ReceipientEmail = recepientMailbox.Name,
                    RawMessage = MailText
                        .Replace("[username]", recepientMailbox.Name)
                        .Replace("[email]", recepientMailbox.Name)
                        .Replace("[url]", claimsUrl)
                        .Replace("[stage]", claimsInvestigation.InvestigationCaseSubStatus.Name)
                        .Replace("[policy]", claimsInvestigation.PolicyDetail.ContractNumber)
                        .Replace("[logo]",
                        claimsInvestigation.PolicyDetail?.DocumentImage != null ?
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(claimsInvestigation.PolicyDetail?.DocumentImage))
                        : "/img/no-image.png")
                };
                recepientMailbox?.Inbox.Add(contactMessage);
                _context.Mailbox.Attach(recepientMailbox);
                _context.Mailbox.Update(recepientMailbox);
                if (company.SendSMS)
                {
                    var result = SmsService.SendSingleMessage(user.PhoneNumber, "Claim(s) allocated: Policy #" + claimsInvestigation.PolicyDetail.ContractNumber + " ");
                }
            }
            try
            {
                var rows = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}