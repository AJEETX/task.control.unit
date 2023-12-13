using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class VendorApplicationUser : ApplicationUser
    {
        [Display(Name = "Vendor code")]
        public string? VendorId { get; set; } = default!;
        public Vendor? Vendor { get; set; } = default!;
        public string? Comments { get; set; } = default!;
    }
}
