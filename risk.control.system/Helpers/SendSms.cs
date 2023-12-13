namespace risk.control.system.Helpers
{
    public class SendSms
    {
        public static Dictionary<string, object> SendSingleMessage(string mobile = "61432854196", string message = "Testing fom Azy", string device = "0", long? timestamp = null, bool isMMS = false, string? attachments = null, bool priority = false)
        {
            // Send a message using the primary device.
            var result = SMS.API.SendSingleMessage("+" + mobile, message, device, timestamp, isMMS, attachments, priority);
            return result;
        }

        #region more options

        public void Send(string mobile = "+61432854196", string message = "Testing fom Azy")
        {
            // Send a message using the primary device.
            SMS.API.SendSingleMessage(mobile, message);

            // Send a message using the Device ID 1.
            Dictionary<string, object> message0 = SMS.API.SendSingleMessage(mobile, message, "0");

            // Send a prioritize message using Device ID 1 for purpose of sending OTP, message reply etc…
            Dictionary<string, object> message1 = SMS.API.SendSingleMessage(mobile, message, "0", null, false, null, true);

            // Send a MMS message using the Device ID 1.
            string attachments = "https://example.com/images/footer-logo.png,https://example.com/downloads/sms-gateway/images/section/create-chat-bot.png";
            Dictionary<string, object> message2 = SMS.API.SendSingleMessage(mobile, message, "0", null, true, attachments);

            // Send a message using the SIM in slot 1 of Device ID 1 (Represented as "1|0").
            // SIM slot is an index so the index of the first SIM is 0 and the index of the second SIM is 1.
            // In this example, 1 represents Device ID and 0 represents SIM slot index.
            Dictionary<string, object> message3 = SMS.API.SendSingleMessage(mobile, message, "1|0");

            // Send scheduled message using the primary device.
            long timestamp = (long)DateTime.UtcNow.AddMinutes(2).Subtract(DateTime.UtcNow).TotalSeconds;
            Dictionary<string, object> message4 = SMS.API.SendSingleMessage(mobile, message, null, timestamp);
        }

        #endregion more options
    }
}