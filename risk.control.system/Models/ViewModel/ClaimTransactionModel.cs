namespace risk.control.system.Models.ViewModel
{
    public class ClaimTransactionModel
    {
        public ClaimsInvestigation Claim { get; set; }
        public CaseLocation Location { get; set; }
        public List<InvestigationTransaction> Log { get; set; }
    }
}