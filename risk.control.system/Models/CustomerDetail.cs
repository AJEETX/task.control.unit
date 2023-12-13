using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class CustomerDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CustomerDetailId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Customer name")]
        public string CustomerName { get; set; }

        [Required]
        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        public DateTime CustomerDateOfBirth { get; set; } = DateTime.UtcNow;

        public Gender? Gender { get; set; }

        [Required]
        [Display(Name = "Customer contact number")]
        [DataType(DataType.PhoneNumber)]
        public long ContactNumber { get; set; }

        [Required]
        [Display(Name = "Address line")]
        public string Addressline { get; set; }

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

        [Required]
        [Display(Name = "Customer type")]
        public CustomerType? CustomerType { get; set; }

        [Required]
        [Display(Name = "Customer income")]
        public Income? CustomerIncome { get; set; }

        [Required]
        [Display(Name = "Customer occupation")]
        public Occupation? CustomerOccupation { get; set; }

        [Required]
        [Display(Name = "Customer education")]
        public Education? CustomerEducation { get; set; }

        [FileExtensions(Extensions = "jpg,jpeg,png")]
        public string? ProfilePictureUrl { get; set; }

        public byte[]? ProfilePicture { get; set; }

        [Display(Name = "Image")]
        [NotMapped]
        public IFormFile? ProfileImage { get; set; }

        public string? Description { get; set; }
    }
}