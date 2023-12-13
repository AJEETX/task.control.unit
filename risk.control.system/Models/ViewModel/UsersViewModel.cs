using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models.ViewModel
{
    public class UsersViewModel
    {
        public string UserId { get; set; }
        public string ProfileImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string? PinCodeId { get; set; }
        public string? PinCode { get; set; }
        public string? StateId { get; set; }
        public string? State { get; set; }
        public string? DistrictId { get; set; }
        public string? District { get; set; }
        public string? Country { get; set; }
        public string? CountryId { get; set; }
        public string Email { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Addressline { get; set; }
        public bool Active { get; set; }
        public IEnumerable<string> Roles { get; set; }

        [Display(Name = "Profile")]
        [NotMapped]
        public IFormFile? Profile { get; set; }

        [Display(Name = "Profile")]
        public byte[]? ProfileImageInByte { get; set; } = default!;
    }

    public class VendorUsersViewModel
    {
        public Vendor Vendor { get; set; }
        public List<UsersViewModel> Users { get; set; } = new();
    }

    public class CompanyUsersViewModel
    {
        public ClientCompany Company { get; set; }
        public List<UsersViewModel> Users { get; set; } = new();
    }
}