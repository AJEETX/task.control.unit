using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class CostCentre : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CostCentreId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "CostCentre name")]
        [Required]
        public string Name { get; set; } = default!;
        [Display(Name = "CostCentre code")]
        [Required]
        public string Code { get; set; } = default!;
    }
}
