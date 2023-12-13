using System.Data;

namespace risk.control.system.Models.ViewModel
{
    public class PinCodeDetails
    {
        public DataTable DataTable { get; set; }
    }

    public class PinCodeState
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string District { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}