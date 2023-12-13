using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class InvestigationCaseSubStatus : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string InvestigationCaseSubStatusId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "Case sub status")]
        [Required]
        public string Name { get; set; } = default!;
        [Display(Name = "Case sub status")]
        [Required]
        public string Code { get; set; } = default!;
        [Display(Name = "Case status")]
        public string? InvestigationCaseStatusId { get; set; }
        public InvestigationCaseStatus? InvestigationCaseStatus { get; set; }
        public bool MasterData { get; set; } = false;
    }
}
