using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class PinCode : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string PinCodeId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "PinCode name")]
        public string Name { get; set; } = default!;
        [Display(Name = "PinCode")]
        public string Code { get; set; } = default!;
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        [Display(Name = "District")]
        public string? DistrictId { get; set; } = default!;
        [Display(Name = "District")]
        public District? District { get; set; } = default!;
        [Display(Name = "State name")]
        public string? StateId { get; set; } = default!;
        [Display(Name = "State name")]
        public State? State { get; set; } = default!;
        [Required]
        [Display(Name = "Country name")]
        public string CountryId { get; set; } = default!;
        [Display(Name = "Country name")]
        public Country Country { get; set; } = default!;
    }
}
