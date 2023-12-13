using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class District : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string DistrictId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "District name")]
        public string Name { get; set; } = default!;
        [Display(Name = "District code")]
        [Required]
        public string Code { get; set; } = default!;
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
