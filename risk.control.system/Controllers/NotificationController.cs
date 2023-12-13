using System.Net.Mail;

using Microsoft.AspNetCore.Mvc;

using risk.control.system.AppConstant;
using risk.control.system.Models.ViewModel;

namespace risk.control.system.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task SendWelcomeEmailAsync(WelcomeRequest request)
        {
            string FilePath = Directory.GetCurrentDirectory() + "\\Files\\Welcome.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[username]", request.UserName).Replace("[email]", request.ToEmail);
            var message = new MailMessage
            {
                Body = MailText,
                From = new MailAddress(Applicationsettings.PORTAL_ADMIN.EMAIL),

            };
            using var smtp = new SmtpClient();
        }
    }
}
