namespace risk.control.system.Models.ViewModel
{
    public class ClaimsInvestigationVendorsModel
    {
        public ClaimsInvestigation ClaimsInvestigation { get; set; }
        public CaseLocation CaseLocation { get; set; }
        public List<VendorCaseModel> Vendors { get; set; }
    }

    public class VendorCaseModel
    {
        public int CaseCount { get; set; }
        public Vendor Vendor { get; set; }
    }
}