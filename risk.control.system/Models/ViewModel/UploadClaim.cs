using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models.ViewModel
{
    public class UploadClaim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Policy { get; set; }
        public string Amount { get; set; }
        public string IssueDate { get; set; }
        public string ClaimType { get; set; }
        public string ServiceType { get; set; }
        public string DateOfIncident { get; set; }
        public string CauseOfLoss { get; set; }
        public string Reason { get; set; }
        public string Dept { get; set; }
        public string? PDocument { get; set; }
        public string Name { get; set; }
        public string CustomerType { get; set; }
        public string Gender { get; set; }
        public string CustomerDob { get; set; }
        public string Contact { get; set; }
        public string Education { get; set; }
        public string Occupation { get; set; }
        public string Income { get; set; }
        public string CAddress { get; set; }
        public string Pincode { get; set; }
        public string Comment { get; set; }
        public string? CPhoto { get; set; }
        public string BeneficiaryName { get; set; }
        public string Relation { get; set; }
        public string BeneficiaryDob { get; set; }
        public string BeneficiaryIncome { get; set; }
        public string BeneficiaryContact { get; set; }
        public string BAddress { get; set; }
        public string BPincode { get; set; }
        public string? BPhoto { get; set; }
    }
}