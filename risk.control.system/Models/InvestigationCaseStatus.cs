using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class InvestigationCaseStatus : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string InvestigationCaseStatusId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "Case status")]
        [Required]
        public string Name { get; set; } = default!;
        [Display(Name = "Case status")]
        [Required]
        public string Code { get; set; } = default!;
        public List<InvestigationCaseSubStatus>? InvestigationCaseSubStatuses { get; set; } = default!;
        public bool MasterData { get; set; } = false;

    }
}
