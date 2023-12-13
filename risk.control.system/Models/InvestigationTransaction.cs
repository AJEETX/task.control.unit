using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class InvestigationTransaction : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string InvestigationTransactionId { get; set; } = Guid.NewGuid().ToString();

        public string? ClaimsInvestigationId { get; set; }
        public virtual ClaimsInvestigation? ClaimsInvestigation { get; set; }
        public string? InvestigationCaseStatusId { get; set; }
        public InvestigationCaseStatus? InvestigationCaseStatus { get; set; }
        public string? InvestigationCaseSubStatusId { get; set; }
        public InvestigationCaseSubStatus? InvestigationCaseSubStatus { get; set; }
        public int? Time2Update { get; set; } = int.MinValue;
        public int? HopCount { get; set; } = 0;
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public string? Message { get; set; }
        public string? headerIcon { get; set; }
        public string? headerMessage { get; set; }
        public string? messageIcon { get; set; }
        public string? footerIcon { get; set; }
        public string? footerMessage { get; set; }
        public string? CurrentClaimOwner { get; set; }
    }
}