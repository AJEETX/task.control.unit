using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class InvestigationCaseOutcome : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string InvestigationCaseOutcomeId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "Case outcome")]
        [Required]
        public string Name { get; set; } = default!;
        [Display(Name = "Case outcome")]
        [Required]
        public string Code { get; set; } = default!;
    }
}
