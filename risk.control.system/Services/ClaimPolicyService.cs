using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

namespace risk.control.system.Services
{
    public interface IClaimPolicyService
    {
        ClaimsInvestigation AddClaimPolicy(string userEmail);

        Task<ClaimTransactionModel> GetClaimDetail(string id);
    }

    public class ClaimPolicyService : IClaimPolicyService
    {
        private readonly ApplicationDbContext _context;

        public ClaimPolicyService(ApplicationDbContext context)
        {
            this._context = context;
        }

        public ClaimsInvestigation AddClaimPolicy(string userEmail)
        {
            var lineOfBusinessId = _context.LineOfBusiness.FirstOrDefault(l => l.Name.ToLower() == "claims").LineOfBusinessId;

            var random = new Random();
            var cNumber = random.Next(3333, 9999);
            var contractNumber = "POLX" + cNumber;

            var existingContractNumber = _context.PolicyDetail.Any(p => p.ContractNumber == contractNumber);
            if (existingContractNumber)
            {
                cNumber = cNumber + random.Next(3333, 9999);
            }
            var model = new ClaimsInvestigation
            {
                PolicyDetail = new PolicyDetail
                {
                    LineOfBusinessId = lineOfBusinessId,
                    CaseEnablerId = _context.CaseEnabler.FirstOrDefault().CaseEnablerId,
                    CauseOfLoss = "LOST IN ACCIDENT",
                    ClaimType = ClaimType.DEATH,
                    ContractIssueDate = DateTime.UtcNow.AddDays(-10),
                    CostCentreId = _context.CostCentre.FirstOrDefault().CostCentreId,
                    DateOfIncident = DateTime.UtcNow.AddDays(-3),
                    InvestigationServiceTypeId = _context.InvestigationServiceType.FirstOrDefault(i => i.Code == "COMP").InvestigationServiceTypeId,
                    Comments = "SOMETHING FISHY",
                    SumAssuredValue = random.Next(100000, 9999999),
                    ContractNumber = "POLX" + cNumber,
                }
            };

            var clientCompanyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            model.PolicyDetail.ClientCompanyId = clientCompanyUser.ClientCompanyId;
            return model;
        }

        public async Task<ClaimTransactionModel> GetClaimDetail(string id)
        {
            var caseLogs = await _context.InvestigationTransaction
                .Include(i => i.InvestigationCaseStatus)
                .Include(i => i.InvestigationCaseSubStatus)
                .Include(c => c.ClaimsInvestigation)
                .ThenInclude(i => i.CaseLocations)
                .Include(c => c.ClaimsInvestigation)
                .ThenInclude(i => i.InvestigationCaseStatus)
                .Include(c => c.ClaimsInvestigation)
                .ThenInclude(i => i.InvestigationCaseSubStatus)
                .Where(t => t.ClaimsInvestigationId == id)
                .OrderByDescending(c => c.HopCount)?.ToListAsync();

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
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.Country)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.Vendor)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.BeneficiaryRelation)
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
                .FirstOrDefaultAsync(m => m.ClaimsInvestigationId == id);

            var location = claimsInvestigation.CaseLocations.FirstOrDefault();

            var model = new ClaimTransactionModel
            {
                Claim = claimsInvestigation,
                Log = caseLogs,
                Location = location
            };
            return model;
        }
    }
}