namespace risk.control.system.Models.ViewModel
{
    public class SMSBody
    {
        public List<Message> messages { get; set; }
    }

    public class Message
    {
        public string channel { get; set; }
        public string originator { get; set; }
        public List<string> recipients { get; set; }
        public string content { get; set; }
        public string data_coding { get; set; }
    }
}