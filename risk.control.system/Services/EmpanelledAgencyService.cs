using Microsoft.EntityFrameworkCore;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

namespace risk.control.system.Services
{
    public interface IEmpanelledAgencyService
    {
        Task<(ClaimsInvestigation, CaseLocation, List<VendorCaseModel>)> GetEmpanelledVendors(string selectedcase);

        Task<ClaimsInvestigation> GetAllocateToVendor(string selectedcase);

        Task<ClaimsInvestigation> GetReAllocateToVendor(string selectedcase);

        ClaimsInvestigation GetCaseLocation(string id);
    }

    public class EmpanelledAgencyService : IEmpanelledAgencyService
    {
        private readonly ApplicationDbContext _context;

        public EmpanelledAgencyService(ApplicationDbContext context)
        {
            this._context = context;
        }

        public ClaimsInvestigation GetCaseLocation(string id)
        {
            var claim = _context.ClaimsInvestigation
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.ClientCompany)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.CaseEnabler)
             .Include(c => c.CaseLocations)
             .ThenInclude(c => c.InvestigationCaseSubStatus)
             .Include(c => c.CaseLocations)
             .ThenInclude(c => c.PinCode)
             .Include(c => c.CaseLocations)
             .ThenInclude(c => c.Vendor)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.CostCentre)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.Country)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.District)
             .Include(c => c.InvestigationCaseStatus)
             .Include(c => c.InvestigationCaseSubStatus)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.InvestigationServiceType)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.LineOfBusiness)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.PinCode)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.State)
             .FirstOrDefault(a => a.ClaimsInvestigationId == id);
            return claim;
        }

        public async Task<ClaimsInvestigation> GetAllocateToVendor(string selectedcase)
        {
            var claimsInvestigation = await _context.ClaimsInvestigation
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.ClientCompany)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.CaseEnabler)
             .Include(c => c.CaseLocations)
             .ThenInclude(c => c.InvestigationCaseSubStatus)
             .Include(c => c.CaseLocations)
             .ThenInclude(c => c.PinCode)
             .Include(c => c.CaseLocations)
             .ThenInclude(c => c.Vendor)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.CostCentre)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.Country)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.District)
             .Include(c => c.InvestigationCaseStatus)
             .Include(c => c.InvestigationCaseSubStatus)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.InvestigationServiceType)
             .Include(c => c.PolicyDetail)
             .ThenInclude(c => c.LineOfBusiness)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.PinCode)
             .Include(c => c.CustomerDetail)
             .ThenInclude(c => c.State)
             .FirstOrDefaultAsync(m => m.ClaimsInvestigationId == selectedcase);

            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var caseLocations = claimsInvestigation.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
            && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId).ToList();

            claimsInvestigation.CaseLocations = caseLocations;
            return claimsInvestigation;
        }

        public async Task<(ClaimsInvestigation, CaseLocation, List<VendorCaseModel>)> GetEmpanelledVendors(string selectedcase)
        {
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);

            var claimsInvestigation = await _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.Vendor)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.Country)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.District)
                .Include(c => c.InvestigationCaseStatus)
                .Include(c => c.InvestigationCaseSubStatus)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.InvestigationServiceType)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.LineOfBusiness)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.State)
                .FirstOrDefaultAsync(m => m.ClaimsInvestigationId == selectedcase);

            var caseLocations = claimsInvestigation.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
            && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId).ToList();

            claimsInvestigation.CaseLocations = caseLocations;

            var location = claimsInvestigation.CaseLocations.FirstOrDefault()?.CaseLocationId;

            var claimCase = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.PinCode)
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.Country)
                .FirstOrDefault(c => c.CaseLocationId == location
                //&& c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId
                );

            var existingVendors = await _context.Vendor
                .Where(c => c.Clients.Any(c => c.ClientCompanyId == claimCase.ClaimsInvestigation.PolicyDetail.ClientCompanyId))
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.District)
                .Include(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.District)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.LineOfBusiness)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.InvestigationServiceType)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.PincodeServices)
                .ToListAsync();

            var allocatedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR);
            var assignedToAgentStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT);
            var submitted2SuperStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR);

            var claimsCases = _context.ClaimsInvestigation
                .Include(c => c.Vendors)
                .Include(c => c.CaseLocations.Where(c =>
                !string.IsNullOrWhiteSpace(c.VendorId) &&
                (c.InvestigationCaseSubStatusId == allocatedStatus.InvestigationCaseSubStatusId ||
                                    c.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId ||
                                    c.InvestigationCaseSubStatusId == submitted2SuperStatus.InvestigationCaseSubStatusId)
                ));

            var vendorCaseCount = new Dictionary<string, int>();

            int countOfCases = 0;
            foreach (var claimsCase in claimsCases)
            {
                if (claimsCase.CaseLocations.Count > 0)
                {
                    foreach (var CaseLocation in claimsCase.CaseLocations)
                    {
                        if (!string.IsNullOrEmpty(CaseLocation.VendorId))
                        {
                            if (CaseLocation.InvestigationCaseSubStatusId == allocatedStatus.InvestigationCaseSubStatusId ||
                                    CaseLocation.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId ||
                                    CaseLocation.InvestigationCaseSubStatusId == submitted2SuperStatus.InvestigationCaseSubStatusId
                                    )
                            {
                                if (!vendorCaseCount.TryGetValue(CaseLocation.VendorId, out countOfCases))
                                {
                                    vendorCaseCount.Add(CaseLocation.VendorId, 1);
                                }
                                else
                                {
                                    int currentCount = vendorCaseCount[CaseLocation.VendorId];
                                    ++currentCount;
                                    vendorCaseCount[CaseLocation.VendorId] = currentCount;
                                }
                            }
                        }
                    }
                }
            }

            List<VendorCaseModel> vendorWithCaseCounts = new();

            foreach (var existingVendor in existingVendors)
            {
                var vendorCase = vendorCaseCount.FirstOrDefault(v => v.Key == existingVendor.VendorId);
                if (vendorCase.Key == existingVendor.VendorId)
                {
                    vendorWithCaseCounts.Add(new VendorCaseModel
                    {
                        CaseCount = vendorCase.Value,
                        Vendor = existingVendor,
                    });
                }
                else
                {
                    vendorWithCaseCounts.Add(new VendorCaseModel
                    {
                        CaseCount = 0,
                        Vendor = existingVendor,
                    });
                }
            }

            return (claimsInvestigation, claimCase, vendorWithCaseCounts);
        }

        public async Task<ClaimsInvestigation> GetReAllocateToVendor(string selectedcase)
        {
            var claimsInvestigation = await _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.Vendor)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.Country)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.District)
                .Include(c => c.InvestigationCaseStatus)
                .Include(c => c.InvestigationCaseSubStatus)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.InvestigationServiceType)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.LineOfBusiness)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CustomerDetail)
                .ThenInclude(c => c.State)
                .FirstOrDefaultAsync(m => m.ClaimsInvestigationId == selectedcase);

            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);
            var caseLocations = claimsInvestigation.CaseLocations.Where(c => !string.IsNullOrWhiteSpace(c.VendorId)
            && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId).ToList();

            claimsInvestigation.CaseLocations = caseLocations;
            return claimsInvestigation;
        }
    }
}