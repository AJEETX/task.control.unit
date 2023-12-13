using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace risk.control.system.Models
{
    public partial class ApplicationUser : IdentityUser<long>
    {
        [FileExtensions(Extensions = "jpg,jpeg,png")]
        public string? ProfilePictureUrl { get; set; }

        public bool IsSuperAdmin { get; set; } = false;
        public bool IsClientAdmin { get; set; } = false;
        public bool IsVendorAdmin { get; set; } = false;
        public byte[]? ProfilePicture { get; set; }

        [Display(Name = "Image")]
        [NotMapped]
        public IFormFile? ProfileImage { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = default!;

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = default!;

        [Display(Name = "PinCode name")]
        public string? PinCodeId { get; set; } = default!;

        [Display(Name = "PinCode name")]
        public PinCode? PinCode { get; set; } = default!;

        [Display(Name = "State name")]
        public string? StateId { get; set; } = default!;

        [Display(Name = "State name")]
        public State? State { get; set; } = default!;

        [Display(Name = "Country name")]
        public string? CountryId { get; set; } = default!;

        [Display(Name = "Country name")]
        public Country? Country { get; set; } = default!;

        [Display(Name = "District")]
        public string? DistrictId { get; set; } = default!;

        [Display(Name = "District")]
        public District? District { get; set; } = default!;

        [Display(Name = "Address line")]
        public string? Addressline { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Updated { get; set; }
        public string? UpdatedBy { get; set; } = default!;

        [Required]
        public string? Password { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; } = false;

        public Mailbox? Mailbox { get; set; } = new();

        public List<ApplicationRole> ApplicationRoles { get; set; } = new();

        public bool Deleted { get; set; } = false;

        public string? SecretPin { get; set; }
        public string? MobileUId { get; set; }
    }

    public class ApplicationRole : IdentityRole<long>
    {
        public ApplicationRole()
        {
            
        }
        public ApplicationRole(string code, string name)
        {
            Name = name;
            Code = code;
        }

        [StringLength(20)]
        public string Code { get; set; }

        public long? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}