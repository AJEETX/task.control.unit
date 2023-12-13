using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class InvestigationServiceType : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string InvestigationServiceTypeId { get; set; } = Guid.NewGuid().ToString();
        [Display(Name = "InvestigationService Type")]
        [Required]
        public string Name { get; set; } = default!;
        [Display(Name = "InvestigationService Type code")]
        [Required]
        public string Code { get; set; } = default!;
        [Display(Name = "Line Of Business")]
        public string LineOfBusinessId { get; set; } = default!;
        public LineOfBusiness LineOfBusiness { get; set; } = default!;
        public bool MasterData { get; set; } = false;
    }
}
