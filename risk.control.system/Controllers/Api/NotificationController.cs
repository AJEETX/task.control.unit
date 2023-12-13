using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using risk.control.system.Helpers;

namespace risk.control.system.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("sms")]
        public async Task<IActionResult> SendSingleSMS(string mobile = "61432854196", string message = "Testing fom Azy")
        {
            string device = "0";
            long? timestamp = null;
            bool isMMS = false;
            string? attachments = null;
            bool priority = false;
            var response = SMS.API.SendSingleMessage("+" + mobile, message, device, timestamp, isMMS, attachments, priority);
            return Ok(response);
        }
    }
}