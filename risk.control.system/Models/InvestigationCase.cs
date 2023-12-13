using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class InvestigationCase : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string InvestigationId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        [Display(Name = "Line of Business")]
        public string? LineOfBusinessId { get; set; } = default!;
        [Display(Name = "Line of Business")]
        public LineOfBusiness? LineOfBusiness { get; set; } = default!;
        public string? InvestigationServiceTypeId { get; set; } = default!;
        public InvestigationServiceType? InvestigationServiceType { get; set; } = default!;
        [Display(Name = "Case status")]
        public string? InvestigationCaseStatusId { get; set; } = default!;
        [Display(Name = "Case status")]
        public InvestigationCaseStatus? InvestigationCaseStatus { get; set; } = default!;
    }

}
