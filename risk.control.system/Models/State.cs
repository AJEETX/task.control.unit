using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class State : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string StateId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "State name")]
        public string Name { get; set; } = default!;
        [Display(Name = "State code")]
        [Required]
        public string Code { get; set; } = default!;
        [Required]
        [Display(Name = "Country name")]
        public string CountryId { get; set; } = default!;
        [Display(Name = "Country name")]
        public Country Country { get; set; } = default!;
    }

}
