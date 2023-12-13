using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class PolicyDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string PolicyDetailId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Display(Name = "Company name")]
        public string? ClientCompanyId { get; set; }

        [Display(Name = "Company name")]
        public ClientCompany? ClientCompany { get; set; }

        [Display(Name = "Line of Business")]
        public string? LineOfBusinessId { get; set; } = default!;

        [Display(Name = "Line of Business")]
        public LineOfBusiness? LineOfBusiness { get; set; } = default!;

        [Display(Name = "Investigation type")]
        public string? InvestigationServiceTypeId { get; set; } = default!;

        [Display(Name = "Investigation type")]
        public InvestigationServiceType? InvestigationServiceType { get; set; } = default!;

        [Display(Name = "Contract number")]
        public string ContractNumber { get; set; } = default!;

        [Required]
        [Display(Name = "Case issue date")]
        [DataType(DataType.Date)]
        public DateTime ContractIssueDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Claim type")]
        public ClaimType? ClaimType { get; set; }

        [Required]
        [Display(Name = "Date of incident")]
        [DataType(DataType.Date)]
        public DateTime DateOfIncident { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Cause of loss")]
        public string CauseOfLoss { get; set; }

        [Required]
        [Display(Name = "Sum assured value")]
        [Column(TypeName = "decimal(15,2)")]
        public decimal SumAssuredValue { get; set; }

        [Required]
        [Display(Name = "Budget centre")]
        public string CostCentreId { get; set; }

        [Display(Name = "Budget centre")]
        public CostCentre? CostCentre { get; set; }

        public string? CaseEnablerId { get; set; }

        [Display(Name = "Reason To Verify")]
        public CaseEnabler? CaseEnabler { get; set; }

        [Display(Name = "Claim Document")]
        [NotMapped]
        public IFormFile? Document { get; set; }

        [Display(Name = "Claim Document")]
        public byte[]? DocumentImage { get; set; } = default!;

        [Display(Name = "Claim remarks")]
        public string? Comments { get; set; }
    }
}