namespace risk.control.system.Models.ViewModel
{
    public class AgentClaimsModel
    {
        public VendorApplicationUser User { get; set; }
        public List<ClaimsInvestigation> Claims { get; set; }
    }
}