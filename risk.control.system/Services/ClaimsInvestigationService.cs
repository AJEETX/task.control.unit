using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using NuGet.Packaging.Signing;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

using System.Data;
using System.IO;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace risk.control.system.Services
{
    public interface IClaimsInvestigationService
    {
        List<ClaimsInvestigation> GetAll();

        Task<ClaimsInvestigation> CreatePolicy(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument, IFormFile? customerDocument, bool create = true);

        Task<ClaimsInvestigation> EdiPolicy(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument);

        Task<ClaimsInvestigation> CreateCustomer(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument, IFormFile? customerDocument, bool create = true);

        Task<ClaimsInvestigation> EditCustomer(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? customerDocument);

        Task Create(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument, IFormFile? customerDocument, bool create = true);

        Task AssignToAssigner(string userEmail, List<string> claimsInvestigations);

        Task<ClaimsInvestigation> AllocateToVendor(string userEmail, string claimsInvestigationId, string vendorId, long caseLocationId);

        Task<ClaimsInvestigation> AssignToVendorAgent(string vendorAgentEmail, string currentUser, string vendorId, string claimsInvestigationId);

        Task<ClaimsInvestigation> SubmitToVendorSupervisor(string userEmail, long caseLocationId, string claimsInvestigationId, string remarks, string? question1, string? question2, string? question3, string? question4);

        Task<ClaimsInvestigation> ProcessAgentReport(string userEmail, string supervisorRemarks, long caseLocationId, string claimsInvestigationId, SupervisorRemarkType remarks);

        Task<ClaimsInvestigation> ProcessCaseReport(string userEmail, string assessorRemarks, long caseLocationId, string claimsInvestigationId, AssessorRemarkType assessorRemarkType);

        List<VendorCaseModel> GetAgencyLoad(List<Vendor> existingVendors);

        Task<List<string>> ProcessAutoAllocation(List<string> claims, ClientCompany company, string userEmail);
    }

    public class ClaimsInvestigationService : IClaimsInvestigationService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IMailboxService mailboxService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ClaimsInvestigationService(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager,
            IMailboxService mailboxService, IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            this._context = context;
            this.roleManager = roleManager;
            this.mailboxService = mailboxService;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<string>> ProcessAutoAllocation(List<string> claims, ClientCompany company, string userEmail)
        {
            var autoAllocatedClaims = new List<string>();
            foreach (var claim in claims)
            {
                string pinCode2Verify = string.Empty;
                //1. GET THE PINCODE FOR EACH CLAIM
                var claimsInvestigation = _context.ClaimsInvestigation
                    .Include(c => c.PolicyDetail)
                    .Include(c => c.CustomerDetail)
                    .ThenInclude(c => c.PinCode)
                    .First(c => c.ClaimsInvestigationId == claim);
                var beneficiary = _context.CaseLocation.Include(b => b.PinCode).FirstOrDefault(b => b.ClaimsInvestigationId == claim);

                if (claimsInvestigation.PolicyDetail?.ClaimType == ClaimType.HEALTH)
                {
                    pinCode2Verify = claimsInvestigation.CustomerDetail?.PinCode?.Code;
                }
                else
                {
                    pinCode2Verify = beneficiary.PinCode?.Code;
                }

                var vendorsInPincode = new List<Vendor>();

                //2. GET THE VENDORID FOR EACH CLAIM BASED ON PINCODE
                foreach (var empanelledVendor in company.EmpanelledVendors)
                {
                    foreach (var serviceType in empanelledVendor.VendorInvestigationServiceTypes)
                    {
                        if (serviceType.InvestigationServiceTypeId == claimsInvestigation.PolicyDetail.InvestigationServiceTypeId &&
                                serviceType.LineOfBusinessId == claimsInvestigation.PolicyDetail.LineOfBusinessId)
                        {
                            foreach (var pincodeService in serviceType.PincodeServices)
                            {
                                if (pincodeService.Pincode == pinCode2Verify)
                                {
                                    vendorsInPincode.Add(empanelledVendor);
                                    continue;
                                }
                            }
                        }
                        var added = vendorsInPincode.Any(v => v.VendorId == empanelledVendor.VendorId);
                        if (added)
                        {
                            continue;
                        }
                    }
                }

                if (vendorsInPincode.Count == 0)
                {
                    foreach (var empanelledVendor in company.EmpanelledVendors)
                    {
                        foreach (var serviceType in empanelledVendor.VendorInvestigationServiceTypes)
                        {
                            if (serviceType.InvestigationServiceTypeId == claimsInvestigation.PolicyDetail.InvestigationServiceTypeId &&
                                    serviceType.LineOfBusinessId == claimsInvestigation.PolicyDetail.LineOfBusinessId)
                            {
                                foreach (var pincodeService in serviceType.PincodeServices)
                                {
                                    if (pincodeService.Pincode.Contains(pinCode2Verify.Substring(0, pinCode2Verify.Length - 2)))
                                    {
                                        vendorsInPincode.Add(empanelledVendor);
                                        continue;
                                    }
                                }
                            }
                            var added = vendorsInPincode.Any(v => v.VendorId == empanelledVendor.VendorId);
                            if (added)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (vendorsInPincode.Count == 0)
                {
                    foreach (var empanelledVendor in company.EmpanelledVendors)
                    {
                        foreach (var serviceType in empanelledVendor.VendorInvestigationServiceTypes)
                        {
                            if (serviceType.InvestigationServiceTypeId == claimsInvestigation.PolicyDetail.InvestigationServiceTypeId &&
                                    serviceType.LineOfBusinessId == claimsInvestigation.PolicyDetail.LineOfBusinessId)
                            {
                                var pincode = _context.PinCode.Include(p => p.District).FirstOrDefault(p => p.Code == pinCode2Verify);
                                if (serviceType.District.DistrictId == pincode.District.DistrictId)
                                {
                                    vendorsInPincode.Add(empanelledVendor);
                                    continue;
                                }
                            }
                            var added = vendorsInPincode.Any(v => v.VendorId == empanelledVendor.VendorId);
                            if (added)
                            {
                                continue;
                            }
                        }
                    }
                }

                var distinctVendors = vendorsInPincode.Distinct()?.ToList();

                //3. CALL SERVICE WITH VENDORID
                if (vendorsInPincode is not null && vendorsInPincode.Count > 0)
                {
                    var vendorsWithCaseLoad = GetAgencyLoad(distinctVendors).OrderBy(o => o.CaseCount)?.ToList();

                    if (vendorsWithCaseLoad is not null && vendorsWithCaseLoad.Count > 0)
                    {
                        var selectedVendor = vendorsWithCaseLoad.FirstOrDefault();

                        var policy = await AllocateToVendor(userEmail, claimsInvestigation.ClaimsInvestigationId, selectedVendor.Vendor.VendorId, beneficiary.CaseLocationId);

                        autoAllocatedClaims.Add(claim);

                        await mailboxService.NotifyClaimAllocationToVendor(userEmail, policy.PolicyDetail.ContractNumber, claimsInvestigation.ClaimsInvestigationId, selectedVendor.Vendor.VendorId, beneficiary.CaseLocationId);
                    }
                }
            }
            return autoAllocatedClaims;
        }

        public List<VendorCaseModel> GetAgencyLoad(List<Vendor> existingVendors)
        {
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
            return vendorWithCaseCounts;
        }

        public async Task<ClaimsInvestigation> CreatePolicy(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument, IFormFile? customerDocument, bool create = true)
        {
            string addedClaimId = string.Empty;
            if (claimsInvestigation is not null)
            {
                try
                {
                    var existingPolicy = await _context.ClaimsInvestigation
                        .Include(c => c.PolicyDetail)
                        .Include(c => c.CustomerDetail)
                        .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId);
                    var currentUser = _context.ClientCompanyApplicationUser.FirstOrDefault(u => u.Email == userEmail);

                    if (existingPolicy != null)
                    {
                        existingPolicy.Updated = DateTime.UtcNow;
                        existingPolicy.UpdatedBy = userEmail;
                        existingPolicy.CurrentUserEmail = userEmail;
                        existingPolicy.CurrentClaimOwner = currentUser.Email;
                        existingPolicy.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId;
                        existingPolicy.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId;
                    }

                    if (claimDocument is not null)
                    {
                        var messageDocumentFileName = Path.GetFileNameWithoutExtension(claimDocument.FileName);
                        var extension = Path.GetExtension(claimDocument.FileName);
                        claimsInvestigation.PolicyDetail.Document = claimDocument;
                        using var dataStream = new MemoryStream();
                        await claimsInvestigation.PolicyDetail.Document.CopyToAsync(dataStream);

                        var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"customer-{DateTime.UtcNow.ToString("dd-MMM-yyyy-HH-mm-ss")}.{extension}");
                        CompressImage.Compressimage(dataStream, filePath);

                        var savedImage = await File.ReadAllBytesAsync(filePath);
                        claimsInvestigation.PolicyDetail.DocumentImage = savedImage;
                    }

                    if (create)
                    {
                        if (claimDocument == null && existingPolicy?.PolicyDetail?.DocumentImage != null)
                        {
                            claimsInvestigation.PolicyDetail.DocumentImage = existingPolicy.PolicyDetail.DocumentImage;
                            claimsInvestigation.PolicyDetail.Document = existingPolicy.PolicyDetail.Document;
                        }
                        claimsInvestigation.Updated = DateTime.UtcNow;
                        claimsInvestigation.UpdatedBy = userEmail;
                        claimsInvestigation.CurrentUserEmail = userEmail;
                        claimsInvestigation.CurrentClaimOwner = currentUser.Email;
                        claimsInvestigation.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId;
                        claimsInvestigation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId;

                        var aaddedClaimId = _context.ClaimsInvestigation.Add(claimsInvestigation);
                        addedClaimId = aaddedClaimId.Entity.ClaimsInvestigationId;
                        if (existingPolicy == null)
                        {
                            var log = new InvestigationTransaction
                            {
                                ClaimsInvestigationId = claimsInvestigation.ClaimsInvestigationId,
                                CurrentClaimOwner = currentUser.Email,
                                Created = DateTime.UtcNow,
                                HopCount = 0,
                                Time2Update = 0,
                                InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId,
                                InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId,
                                UpdatedBy = userEmail
                            };
                            _context.InvestigationTransaction.Add(log);
                        }
                    }
                    else
                    {
                        if (claimDocument == null && existingPolicy.PolicyDetail?.DocumentImage != null)
                        {
                            claimsInvestigation.PolicyDetail.DocumentImage = existingPolicy.PolicyDetail.DocumentImage;
                            claimsInvestigation.PolicyDetail.Document = existingPolicy.PolicyDetail.Document;
                        }
                        var addedClaim = _context.PolicyDetail.Update(claimsInvestigation.PolicyDetail);
                        existingPolicy.PolicyDetail = addedClaim.Entity;
                        existingPolicy.PolicyDetailId = addedClaim.Entity.PolicyDetailId;
                        _context.ClaimsInvestigation.Update(existingPolicy);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return claimsInvestigation;
        }

        public async Task<ClaimsInvestigation> EdiPolicy(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument)
        {
            string addedClaimId = string.Empty;
            if (claimsInvestigation is not null)
            {
                try
                {
                    var existingPolicy = await _context.ClaimsInvestigation
                        .Include(c => c.PolicyDetail)
                            .FirstOrDefaultAsync(c => c.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId);
                    if (existingPolicy != null)
                    {
                        existingPolicy.PolicyDetail.ContractIssueDate = claimsInvestigation.PolicyDetail.ContractIssueDate;
                        existingPolicy.PolicyDetail.InvestigationServiceTypeId = claimsInvestigation.PolicyDetail.InvestigationServiceTypeId;
                        existingPolicy.PolicyDetail.ClaimType = claimsInvestigation.PolicyDetail.ClaimType;
                        existingPolicy.PolicyDetail.CostCentreId = claimsInvestigation.PolicyDetail.CostCentreId;
                        existingPolicy.PolicyDetail.CaseEnablerId = claimsInvestigation.PolicyDetail.CaseEnablerId;
                        existingPolicy.PolicyDetail.DateOfIncident = claimsInvestigation.PolicyDetail.DateOfIncident;
                        existingPolicy.PolicyDetail.ContractNumber = claimsInvestigation.PolicyDetail.ContractNumber;
                        existingPolicy.PolicyDetail.SumAssuredValue = claimsInvestigation.PolicyDetail.SumAssuredValue;
                        existingPolicy.PolicyDetail.CauseOfLoss = claimsInvestigation.PolicyDetail.CauseOfLoss;
                        existingPolicy.Updated = DateTime.UtcNow;
                        existingPolicy.UpdatedBy = userEmail;
                        existingPolicy.CurrentUserEmail = userEmail;
                        existingPolicy.CurrentClaimOwner = userEmail;
                        existingPolicy.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId;
                        existingPolicy.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId;

                        if (claimDocument is not null)
                        {
                            var messageDocumentFileName = Path.GetFileNameWithoutExtension(claimDocument.FileName);
                            var extension = Path.GetExtension(claimDocument.FileName);
                            existingPolicy.PolicyDetail.Document = claimDocument;
                            using var dataStream = new MemoryStream();
                            await existingPolicy.PolicyDetail.Document.CopyToAsync(dataStream);
                            existingPolicy.PolicyDetail.DocumentImage = dataStream.ToArray();
                        }

                        _context.ClaimsInvestigation.Update(existingPolicy);

                        await _context.SaveChangesAsync();

                        return existingPolicy;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return claimsInvestigation;
        }

        public async Task<ClaimsInvestigation> EditCustomer(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? customerDocument)
        {
            if (claimsInvestigation is not null)
            {
                try
                {
                    var existingPolicy = await _context.ClaimsInvestigation
                        .Include(c => c.CustomerDetail)
                            .FirstOrDefaultAsync(c => c.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId);
                    if (existingPolicy != null)
                    {
                        existingPolicy.CustomerDetail.DistrictId = claimsInvestigation.CustomerDetail.DistrictId;
                        existingPolicy.CustomerDetail.Addressline = claimsInvestigation.CustomerDetail.Addressline;
                        existingPolicy.CustomerDetail.Description = claimsInvestigation.CustomerDetail.Description;
                        existingPolicy.CustomerDetail.ContactNumber = claimsInvestigation.CustomerDetail.ContactNumber;
                        existingPolicy.CustomerDetail.CountryId = claimsInvestigation.CustomerDetail.CountryId;
                        existingPolicy.CustomerDetail.CustomerDateOfBirth = claimsInvestigation.CustomerDetail.CustomerDateOfBirth;
                        existingPolicy.CustomerDetail.CustomerEducation = claimsInvestigation.CustomerDetail.CustomerEducation;
                        existingPolicy.CustomerDetail.CustomerIncome = claimsInvestigation.CustomerDetail.CustomerIncome;
                        existingPolicy.CustomerDetail.CustomerName = claimsInvestigation.CustomerDetail.CustomerName;
                        existingPolicy.CustomerDetail.CustomerOccupation = claimsInvestigation.CustomerDetail.CustomerOccupation;
                        existingPolicy.CustomerDetail.CustomerType = claimsInvestigation.CustomerDetail.CustomerType;
                        existingPolicy.CustomerDetail.Gender = claimsInvestigation.CustomerDetail.Gender;
                        existingPolicy.CustomerDetail.PinCodeId = claimsInvestigation.CustomerDetail.PinCodeId;
                        existingPolicy.CustomerDetail.StateId = claimsInvestigation.CustomerDetail.StateId;

                        existingPolicy.Updated = DateTime.UtcNow;
                        existingPolicy.UpdatedBy = userEmail;
                        existingPolicy.CurrentUserEmail = userEmail;
                        existingPolicy.CurrentClaimOwner = userEmail;
                        existingPolicy.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId;
                        existingPolicy.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId;
                        if (customerDocument is not null)
                        {
                            var newFileName = Path.GetFileNameWithoutExtension(customerDocument.FileName) + Guid.NewGuid().ToString();
                            var fileExtension = Path.GetExtension(customerDocument.FileName);
                            newFileName += fileExtension;
                            //var upload = Path.Combine(webHostEnvironment.WebRootPath, "document", newFileName);
                            //customerDocument.CopyTo(new FileStream(upload, FileMode.Create));

                            using var dataStream = new MemoryStream();
                            customerDocument.CopyTo(dataStream);

                            var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"{newFileName}");
                            CompressImage.Compressimage(dataStream, filePath);

                            var savedImage = await File.ReadAllBytesAsync(filePath);
                            existingPolicy.CustomerDetail.ProfilePicture = savedImage;
                            existingPolicy.CustomerDetail.ProfilePictureUrl = "/document/" + newFileName;
                        }
                        var pinCode = _context.PinCode.FirstOrDefault(p => p.PinCodeId == existingPolicy.CustomerDetail.PinCodeId);
                        //var pinCodeData = await httpClientService.GetPinCodeLatLng(pinCode.Code);

                        //existingPolicy.CustomerDetail.PinCode.Latitude = pinCodeData.FirstOrDefault()?.Lat.ToString();
                        //existingPolicy.CustomerDetail.PinCode.Longitude = pinCodeData.FirstOrDefault()?.Lng.ToString();
                        _context.ClaimsInvestigation.Update(existingPolicy);

                        await _context.SaveChangesAsync();

                        return existingPolicy;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return claimsInvestigation;
        }

        public async Task<ClaimsInvestigation> CreateCustomer(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument, IFormFile? customerDocument, bool create = true)
        {
            if (claimsInvestigation is not null)
            {
                try
                {
                    var existingPolicy = await _context.ClaimsInvestigation
                        .Include(c => c.CustomerDetail)
                        .ThenInclude(c => c.PinCode)
                        .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId);
                    if (existingPolicy != null)
                    {
                        existingPolicy.Updated = DateTime.UtcNow;
                        existingPolicy.UpdatedBy = userEmail;
                        existingPolicy.CurrentUserEmail = userEmail;
                        existingPolicy.CurrentClaimOwner = userEmail;
                        existingPolicy.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId;
                        existingPolicy.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId;
                    }

                    if (customerDocument is not null)
                    {
                        var newFileName = Path.GetFileNameWithoutExtension(customerDocument.FileName) + Guid.NewGuid().ToString();
                        var fileExtension = Path.GetExtension(customerDocument.FileName);
                        newFileName += fileExtension;

                        using var dataStream = new MemoryStream();
                        await claimsInvestigation.CustomerDetail.ProfileImage.CopyToAsync(dataStream);

                        var filePath = Path.Combine(webHostEnvironment.WebRootPath, "document", $"{newFileName}");
                        CompressImage.Compressimage(dataStream, filePath);

                        var savedImage = await File.ReadAllBytesAsync(filePath);
                        claimsInvestigation.CustomerDetail.ProfilePicture = savedImage;

                        claimsInvestigation.CustomerDetail.ProfilePictureUrl = "/document/" + $"{newFileName}";
                    }
                    if (create)
                    {
                        if (customerDocument == null && existingPolicy.CustomerDetail?.ProfilePicture != null)
                        {
                            claimsInvestigation.CustomerDetail.ProfilePictureUrl = existingPolicy.CustomerDetail.ProfilePictureUrl;
                            claimsInvestigation.CustomerDetail.ProfilePicture = existingPolicy.CustomerDetail.ProfilePicture;
                            claimsInvestigation.CustomerDetail.ProfileImage = existingPolicy.CustomerDetail.ProfileImage;
                        }
                        var addedClaim = _context.CustomerDetail.Add(claimsInvestigation.CustomerDetail);
                        existingPolicy.CustomerDetail = addedClaim.Entity;
                    }
                    else
                    {
                        if (customerDocument == null && existingPolicy.CustomerDetail?.ProfilePicture != null)
                        {
                            claimsInvestigation.CustomerDetail.ProfilePictureUrl = existingPolicy.CustomerDetail.ProfilePictureUrl;
                            claimsInvestigation.CustomerDetail.ProfilePicture = existingPolicy.CustomerDetail.ProfilePicture;
                            claimsInvestigation.CustomerDetail.ProfileImage = existingPolicy.CustomerDetail.ProfileImage;
                        }

                        var addedClaim = _context.CustomerDetail.Update(claimsInvestigation.CustomerDetail);
                        existingPolicy.CustomerDetail = addedClaim.Entity;
                    }
                    var pincode = _context.PinCode.FirstOrDefault(p => p.PinCodeId == existingPolicy.CustomerDetail.PinCodeId);

                    existingPolicy.CustomerDetail.PinCode.Latitude = pincode.Latitude;
                    existingPolicy.CustomerDetail.PinCode.Longitude = pincode.Longitude;

                    _context.ClaimsInvestigation.Update(existingPolicy);

                    await _context.SaveChangesAsync();
                    return existingPolicy;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return claimsInvestigation;
        }

        public async Task Create(string userEmail, ClaimsInvestigation claimsInvestigation, IFormFile? claimDocument, IFormFile? customerDocument, bool create = true)
        {
            if (claimsInvestigation is not null)
            {
                try
                {
                    claimsInvestigation.Updated = DateTime.UtcNow;
                    claimsInvestigation.UpdatedBy = userEmail;
                    claimsInvestigation.CurrentUserEmail = userEmail;
                    claimsInvestigation.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId;
                    claimsInvestigation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId;
                    if (customerDocument is not null)
                    {
                        var messageDocumentFileName = Path.GetFileNameWithoutExtension(customerDocument.FileName);
                        var extension = Path.GetExtension(customerDocument.FileName);
                        claimsInvestigation.CustomerDetail.ProfileImage = customerDocument;
                        using var dataStream = new MemoryStream();
                        await claimsInvestigation.CustomerDetail.ProfileImage.CopyToAsync(dataStream);
                        claimsInvestigation.CustomerDetail.ProfilePicture = dataStream.ToArray();
                    }

                    if (claimDocument is not null)
                    {
                        var messageDocumentFileName = Path.GetFileNameWithoutExtension(claimDocument.FileName);
                        var extension = Path.GetExtension(claimDocument.FileName);
                        claimsInvestigation.PolicyDetail.Document = claimDocument;
                        using var dataStream = new MemoryStream();
                        await claimsInvestigation.PolicyDetail.Document.CopyToAsync(dataStream);
                        claimsInvestigation.PolicyDetail.DocumentImage = dataStream.ToArray();
                    }

                    if (create)
                    {
                        _context.ClaimsInvestigation.Add(claimsInvestigation);
                        var log = new InvestigationTransaction
                        {
                            ClaimsInvestigationId = claimsInvestigation.ClaimsInvestigationId,
                            Created = DateTime.UtcNow,
                            HopCount = 0,
                            Time2Update = 0,
                            InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INITIATED).InvestigationCaseStatusId,
                            InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR).InvestigationCaseSubStatusId,
                            UpdatedBy = userEmail
                        };

                        _context.InvestigationTransaction.Add(log);
                    }
                    else
                    {
                        var existingClaim = await _context.ClaimsInvestigation.AsNoTracking()
                            .FirstOrDefaultAsync(c => c.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId);
                        if (claimDocument == null && existingClaim.PolicyDetail.DocumentImage != null)
                        {
                            claimsInvestigation.PolicyDetail.DocumentImage = existingClaim.PolicyDetail.DocumentImage;
                            claimsInvestigation.PolicyDetail.Document = existingClaim.PolicyDetail.Document;
                        }
                        if (customerDocument == null && existingClaim.CustomerDetail.ProfilePicture != null)
                        {
                            claimsInvestigation.CustomerDetail.ProfilePictureUrl = existingClaim.CustomerDetail.ProfilePictureUrl;
                            claimsInvestigation.CustomerDetail.ProfilePicture = existingClaim.CustomerDetail.ProfilePicture;
                            claimsInvestigation.CustomerDetail.ProfileImage = existingClaim.CustomerDetail.ProfileImage;
                        }
                        _context.ClaimsInvestigation.Update(claimsInvestigation);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task AssignToAssigner(string userEmail, List<string> claims)
        {
            if (claims is not null && claims.Count > 0)
            {
                var cases2Assign = _context.ClaimsInvestigation
                    .Include(c => c.CaseLocations)
                    .Where(v => claims.Contains(v.ClaimsInvestigationId));

                var assignerRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Assigner.ToString()));

                var currentUser = _context.ClientCompanyApplicationUser.FirstOrDefault(u => u.Email == userEmail);
                var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == currentUser.ClientCompanyId);
                string currentOwner = string.Empty;
                foreach (var companyUser in companyUsers)
                {
                    var isAssigner = await userManager.IsInRoleAsync(companyUser, assignerRole?.Name);
                    if (isAssigner)
                    {
                        currentOwner = companyUser.Email;
                        break;
                    }
                }

                foreach (var claimsInvestigation in cases2Assign)
                {
                    claimsInvestigation.Updated = DateTime.UtcNow;
                    claimsInvestigation.UpdatedBy = currentUser.FirstName + " " + currentUser.LastName + "( " + currentUser.Email + ")";
                    claimsInvestigation.CurrentUserEmail = userEmail;
                    claimsInvestigation.CurrentClaimOwner = currentOwner;
                    claimsInvestigation.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId;
                    claimsInvestigation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER).InvestigationCaseSubStatusId;
                    foreach (var caseLocation in claimsInvestigation.CaseLocations)
                    {
                        caseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER).InvestigationCaseSubStatusId;
                    }

                    var lastLog = _context.InvestigationTransaction
                        .Where(i =>
                            i.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId)
                            .OrderByDescending(o => o.Created)?.FirstOrDefault();

                    var lastLogHop = _context.InvestigationTransaction
                        .Where(i => i.ClaimsInvestigationId == claimsInvestigation.ClaimsInvestigationId)
                        .AsNoTracking().Max(s => s.HopCount);

                    var log = new InvestigationTransaction
                    {
                        HopCount = lastLogHop + 1,
                        Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                        CurrentClaimOwner = currentOwner,
                        ClaimsInvestigationId = claimsInvestigation.ClaimsInvestigationId,
                        Created = DateTime.UtcNow,
                        InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                        InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER).InvestigationCaseSubStatusId,
                        UpdatedBy = currentUser.Email
                    };
                    _context.InvestigationTransaction.Add(log);
                }
                _context.UpdateRange(cases2Assign);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ClaimsInvestigation> AllocateToVendor(string userEmail, string claimsInvestigationId, string vendorId, long caseLocationId)
        {
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == vendorId);
            var currentUser = _context.ClientCompanyApplicationUser.FirstOrDefault(u => u.Email == userEmail);

            var supervisor = await GetSupervisor(vendorId);

            if (vendor != null)
            {
                var claimsCaseLocation = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.InvestigationCaseSubStatus)
                .Include(c => c.Vendor)
                .Include(c => c.PinCode)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.State)
                .FirstOrDefault(c => c.CaseLocationId == caseLocationId && c.ClaimsInvestigationId == claimsInvestigationId);

                claimsCaseLocation.Vendor = vendor;
                claimsCaseLocation.VendorId = vendorId;
                claimsCaseLocation.AssignedAgentUserEmail = supervisor.Email;
                claimsCaseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR).InvestigationCaseSubStatusId;
                _context.CaseLocation.Update(claimsCaseLocation);

                var claimsCaseToAllocateToVendor = _context.ClaimsInvestigation
                    .Include(c => c.PolicyDetail)
                    .FirstOrDefault(v => v.ClaimsInvestigationId == claimsInvestigationId);
                claimsCaseToAllocateToVendor.Updated = DateTime.UtcNow;
                claimsCaseToAllocateToVendor.UpdatedBy = currentUser.FirstName + " " + currentUser.LastName + " (" + currentUser.Email + ")";
                claimsCaseToAllocateToVendor.CurrentUserEmail = userEmail;
                claimsCaseToAllocateToVendor.CurrentClaimOwner = supervisor.Email;
                claimsCaseToAllocateToVendor.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId;
                claimsCaseToAllocateToVendor.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR).InvestigationCaseSubStatusId;
                var existinCaseLocation = claimsCaseToAllocateToVendor.CaseLocations.FirstOrDefault(c => c.CaseLocationId == caseLocationId);
                existinCaseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR).InvestigationCaseSubStatusId;
                claimsCaseToAllocateToVendor.Vendors.Add(vendor);
                _context.ClaimsInvestigation.Update(claimsCaseToAllocateToVendor);
                var lastLog = _context.InvestigationTransaction.Where(i =>
                i.ClaimsInvestigationId == claimsCaseToAllocateToVendor.ClaimsInvestigationId).OrderByDescending(o => o.Created)?.FirstOrDefault();

                var lastLogHop = _context.InvestigationTransaction
                        .Where(i => i.ClaimsInvestigationId == claimsInvestigationId)
                        .AsNoTracking().Max(s => s.HopCount);

                var log = new InvestigationTransaction
                {
                    HopCount = lastLogHop + 1,
                    ClaimsInvestigationId = claimsCaseToAllocateToVendor.ClaimsInvestigationId,
                    CurrentClaimOwner = claimsCaseToAllocateToVendor.CurrentClaimOwner,
                    Created = DateTime.UtcNow,
                    Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                    InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                    InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR).InvestigationCaseSubStatusId,
                    UpdatedBy = currentUser.Email
                };
                _context.InvestigationTransaction.Add(log);

                await _context.SaveChangesAsync();

                return claimsCaseToAllocateToVendor;
            }
            return null;
        }

        public async Task<ClaimsInvestigation> AssignToVendorAgent(string vendorAgentEmail, string currentUser, string vendorId, string claimsInvestigationId)
        {
            var supervisor = await GetSupervisor(vendorId);

            var claim = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.Vendor)
                .Where(c => c.ClaimsInvestigationId == claimsInvestigationId).FirstOrDefault();
            if (claim != null)
            {
                var claimsCaseLocation = _context.CaseLocation
                    .Include(c => c.ClaimsInvestigation)
                    .Include(c => c.InvestigationCaseSubStatus)
                    .Include(c => c.Vendor)
                    .Include(c => c.PinCode)
                    .Include(c => c.District)
                    .Include(c => c.State)
                    .Include(c => c.State)
                    .FirstOrDefault(c => c.VendorId == vendorId && c.ClaimsInvestigationId == claimsInvestigationId);
                claimsCaseLocation.AssignedAgentUserEmail = vendorAgentEmail;
                claimsCaseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT).InvestigationCaseSubStatusId;
                _context.CaseLocation.Update(claimsCaseLocation);

                var agentUser = _context.VendorApplicationUser.FirstOrDefault(u => u.Email == vendorAgentEmail);

                claim.Updated = DateTime.UtcNow;
                claim.UpdatedBy = supervisor.Email;
                claim.CurrentUserEmail = currentUser;
                claim.CurrentClaimOwner = agentUser.Email;
                claim.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId;
                claim.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT).InvestigationCaseSubStatusId;

                var lastLog = _context.InvestigationTransaction.Where(i =>
                i.ClaimsInvestigationId == claim.ClaimsInvestigationId).OrderByDescending(o => o.Created)?.FirstOrDefault();

                var lastLogHop = _context.InvestigationTransaction
                                        .Where(i => i.ClaimsInvestigationId == claim.ClaimsInvestigationId)
                                        .AsNoTracking().Max(s => s.HopCount);

                var log = new InvestigationTransaction
                {
                    HopCount = lastLogHop + 1,
                    ClaimsInvestigationId = claim.ClaimsInvestigationId,
                    CurrentClaimOwner = claim.CurrentClaimOwner,
                    Created = DateTime.UtcNow,
                    Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                    InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                    InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT).InvestigationCaseSubStatusId,
                    UpdatedBy = supervisor.FirstName + " " + supervisor.LastName + " (" + supervisor.Email + ")"
                };
                _context.InvestigationTransaction.Add(log);
            }
            _context.ClaimsInvestigation.Update(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<ClaimsInvestigation> SubmitToVendorSupervisor(string userEmail, long caseLocationId, string claimsInvestigationId, string remarks, string? question1, string? question2, string? question3, string? question4)
        {
            var agent = _context.VendorApplicationUser.FirstOrDefault(a => a.Email.Trim().ToLower() == userEmail.ToLower());

            var supervisor = await GetSupervisor(agent.VendorId);

            var claim = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .FirstOrDefault(c => c.ClaimsInvestigationId == claimsInvestigationId);

            claim.Updated = DateTime.UtcNow;
            claim.UpdatedBy = agent.FirstName + " " + agent.LastName + "(" + agent.Email + ")";
            claim.CurrentUserEmail = userEmail;
            claim.CurrentClaimOwner = supervisor.Email;
            claim.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId;
            claim.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus
                .FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR).InvestigationCaseSubStatusId;

            var caseLocation = _context.CaseLocation
                .Include(c => c.ClaimReport)
                .FirstOrDefault(c => c.CaseLocationId == caseLocationId && c.ClaimsInvestigationId == claimsInvestigationId);

            var claimReport = _context.ClaimReport.FirstOrDefault(c => c.ClaimReportId == caseLocation.ClaimReport.ClaimReportId);

            claimReport.Question1 = question1;

            if (question2 == "0" || question2 == "0.0")
            {
                question2 = "Low";
            }
            else if (question2 == ".5" || question2 == "0.5")
            {
                question2 = "Medium";
            }
            else if (question2 == "1" || question2 == "1.0")
            {
                question2 = "High";
            }

            claimReport.Question2 = question2;
            claimReport.Question3 = question3;
            claimReport.Question4 = question4;
            claimReport.AgentRemarks = remarks;
            claimReport.AgentRemarksUpdated = DateTime.UtcNow;
            claimReport.AgentEmail = userEmail;
            _context.ClaimReport.Update(claimReport);

            caseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus
                .FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR).InvestigationCaseSubStatusId;
            caseLocation.Updated = DateTime.UtcNow;
            caseLocation.UpdatedBy = userEmail;
            caseLocation.AssignedAgentUserEmail = supervisor.Email;
            caseLocation.IsReviewCaseLocation = false;
            _context.CaseLocation.Update(caseLocation);

            var lastLog = _context.InvestigationTransaction.Where(i =>
               i.ClaimsInvestigationId == claimsInvestigationId).OrderByDescending(o => o.Created)?.FirstOrDefault();

            var lastLogHop = _context.InvestigationTransaction
                                       .Where(i => i.ClaimsInvestigationId == claim.ClaimsInvestigationId)
                                       .AsNoTracking().Max(s => s.HopCount);

            var log = new InvestigationTransaction
            {
                ClaimsInvestigationId = claimsInvestigationId,
                HopCount = lastLogHop + 1,
                CurrentClaimOwner = supervisor.Email,
                Created = DateTime.UtcNow,
                Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR).InvestigationCaseSubStatusId,
                UpdatedBy = agent.Email
            };
            _context.InvestigationTransaction.Add(log);

            _context.ClaimsInvestigation.Update(claim);

            try
            {
                await _context.SaveChangesAsync();
                return claim;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ClaimsInvestigation> ProcessAgentReport(string userEmail, string supervisorRemarks, long caseLocationId, string claimsInvestigationId, SupervisorRemarkType reportUpdateStatus)
        {
            if (reportUpdateStatus == SupervisorRemarkType.OK)
            {
                return await ApproveAgentReport(userEmail, claimsInvestigationId, caseLocationId, supervisorRemarks, reportUpdateStatus);
            }
            else
            {
                //PUT th case back in review list :: Assign back to Agent
                return await ReAllocateToVendor(userEmail, claimsInvestigationId, caseLocationId, supervisorRemarks, reportUpdateStatus);
            }
        }

        public async Task<ClaimsInvestigation> ProcessCaseReport(string userEmail, string assessorRemarks, long caseLocationId, string claimsInvestigationId, AssessorRemarkType reportUpdateStatus)
        {
            var claim = _context.ClaimsInvestigation
                 .FirstOrDefault(c => c.ClaimsInvestigationId == claimsInvestigationId);

            if (reportUpdateStatus == AssessorRemarkType.OK)
            {
                return await ApproveCaseReport(userEmail, assessorRemarks, caseLocationId, claimsInvestigationId, reportUpdateStatus);
            }
            else
            {
                //PUT th case back in review list :: Assign back to Agent
                return await ReAssignToAssigner(userEmail, claimsInvestigationId, caseLocationId, assessorRemarks, reportUpdateStatus);
            }
        }

        private async Task<ClaimsInvestigation> ApproveCaseReport(string userEmail, string assessorRemarks, long caseLocationId, string claimsInvestigationId, AssessorRemarkType assessorRemarkType)
        {
            var caseLocation = _context.CaseLocation
                .Include(c => c.ClaimReport)
                .FirstOrDefault(c => c.CaseLocationId == caseLocationId && c.ClaimsInvestigationId == claimsInvestigationId);

            var report = _context.ClaimReport.FirstOrDefault(c => c.ClaimReportId == caseLocation.ClaimReport.ClaimReportId);
            report.AssessorRemarkType = assessorRemarkType;
            report.AssessorRemarks = assessorRemarks;
            report.AssessorRemarksUpdated = DateTime.UtcNow;
            report.AssessorEmail = userEmail;

            _context.ClaimReport.Update(report);
            caseLocation.ClaimReport = report;
            caseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus
                .FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR).InvestigationCaseSubStatusId;
            caseLocation.Updated = DateTime.UtcNow;
            caseLocation.UpdatedBy = userEmail;
            _context.CaseLocation.Update(caseLocation);
            try
            {
                await _context.SaveChangesAsync();
                var claim = _context.ClaimsInvestigation
                    .Include(c => c.PolicyDetail)
                .FirstOrDefault(c => c.ClaimsInvestigationId == claimsInvestigationId);

                if (claim != null && claim.CaseLocations.All(c => c.InvestigationCaseSubStatusId == _context.InvestigationCaseSubStatus
                    .FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR).InvestigationCaseSubStatusId))
                {
                    claim.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.FINISHED).InvestigationCaseStatusId;
                    claim.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR).InvestigationCaseSubStatusId;
                    _context.ClaimsInvestigation.Update(claim);

                    var finalHop = _context.InvestigationTransaction
                                       .Where(i => i.ClaimsInvestigationId == claimsInvestigationId)
                                        .AsNoTracking().Max(s => s.HopCount);

                    var finalLog = new InvestigationTransaction
                    {
                        HopCount = finalHop + 1,
                        ClaimsInvestigationId = claimsInvestigationId,
                        CurrentClaimOwner = claim.CurrentClaimOwner,
                        Created = DateTime.UtcNow,
                        Time2Update = DateTime.UtcNow.Subtract(claim.Created).Days,
                        InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.FINISHED).InvestigationCaseStatusId,
                        InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                        i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR).InvestigationCaseSubStatusId,
                        UpdatedBy = userEmail
                    };

                    _context.InvestigationTransaction.Add(finalLog);

                    return await _context.SaveChangesAsync() > 0 ? claim : null;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        private async Task<VendorApplicationUser> GetSupervisor(string vendorId)
        {
            var vendorNonAdminUsers = _context.VendorApplicationUser.Where(u =>
            u.VendorId == vendorId && !u.IsVendorAdmin);

            var supervisor = roleManager.Roles.FirstOrDefault(r =>
                r.Name.Contains(AppRoles.Supervisor.ToString()));

            foreach (var vendorNonAdminUser in vendorNonAdminUsers)
            {
                if (await userManager.IsInRoleAsync(vendorNonAdminUser, supervisor?.Name))
                {
                    return vendorNonAdminUser;
                }
            }
            return null;
        }

        private async Task<ClaimsInvestigation> ReAssignToAssigner(string userEmail, string claimsInvestigationId, long caseLocationId, string assessorRemarks, AssessorRemarkType assessorRemarkType)
        {
            var claimsCaseLocation = _context.CaseLocation
                .Include(c => c.ClaimReport)
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.InvestigationCaseSubStatus)
                .Include(c => c.Vendor)
                .Include(c => c.PinCode)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.State)
                .FirstOrDefault(c => c.CaseLocationId == caseLocationId && c.ClaimsInvestigationId == claimsInvestigationId);

            var report = _context.ClaimReport.FirstOrDefault(c => c.ClaimReportId == claimsCaseLocation.ClaimReport.ClaimReportId);
            report.AssessorRemarkType = assessorRemarkType;
            report.AssessorRemarks = assessorRemarks;
            report.AssessorRemarksUpdated = DateTime.UtcNow;
            report.AssessorEmail = userEmail;

            _context.ClaimReport.Update(report);
            claimsCaseLocation.ClaimReport = report;
            claimsCaseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                    i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER).InvestigationCaseSubStatusId;
            claimsCaseLocation.IsReviewCaseLocation = true;
            _context.CaseLocation.Update(claimsCaseLocation);

            var claimsCaseToReassign = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .FirstOrDefault(v => v.ClaimsInvestigationId == claimsInvestigationId);
            claimsCaseToReassign.Updated = DateTime.UtcNow;
            claimsCaseToReassign.UpdatedBy = userEmail;
            claimsCaseToReassign.CurrentUserEmail = userEmail;
            claimsCaseToReassign.IsReviewCase = true;
            claimsCaseToReassign.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                    i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER).InvestigationCaseSubStatusId;

            _context.ClaimsInvestigation.Update(claimsCaseToReassign);
            var lastLog = _context.InvestigationTransaction.Where(i =>
                            i.ClaimsInvestigationId == claimsCaseToReassign.ClaimsInvestigationId).OrderByDescending(o => o.Created)?.FirstOrDefault();

            var lastLogHop = _context.InvestigationTransaction
                                       .Where(i => i.ClaimsInvestigationId == claimsInvestigationId)
                .AsNoTracking().Max(s => s.HopCount);

            var log = new InvestigationTransaction
            {
                HopCount = lastLogHop + 1,
                ClaimsInvestigationId = claimsCaseToReassign.ClaimsInvestigationId,
                Created = DateTime.UtcNow,
                Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER).InvestigationCaseSubStatusId,
                UpdatedBy = userEmail
            };
            _context.InvestigationTransaction.Add(log);

            return await _context.SaveChangesAsync() > 0 ? claimsCaseToReassign : null;
        }

        private async Task<ClaimsInvestigation> ApproveAgentReport(string userEmail, string claimsInvestigationId, long caseLocationId, string supervisorRemarks, SupervisorRemarkType reportUpdateStatus)
        {
            var caseLocation = _context.CaseLocation
                .Include(c => c.ClaimReport)
                .FirstOrDefault(c => c.CaseLocationId == caseLocationId && c.ClaimsInvestigationId == claimsInvestigationId);

            var claim = _context.ClaimsInvestigation
                .Include(c => c.Vendor)
                .Include(c => c.PolicyDetail)
                .FirstOrDefault(c => c.ClaimsInvestigationId == claimsInvestigationId);

            var assessorRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Assessor.ToString()));
            var supervisorRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Supervisor.ToString()));

            var clientCompany = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == claim.PolicyDetail.ClientCompanyId);

            var agencyUser = _context.VendorApplicationUser.FirstOrDefault(s => s.Email == userEmail);

            string supervisorUser = string.Empty;

            var isSupervisor = await userManager.IsInRoleAsync(agencyUser, supervisorRole?.Name);
            if (isSupervisor)
            {
                supervisorUser = agencyUser.Email;
            }

            var companyUsers = _context.ClientCompanyApplicationUser.Where(c => c.ClientCompanyId == clientCompany.ClientCompanyId);
            string currentOwner = string.Empty;
            foreach (var companyUser in companyUsers)
            {
                var isAssigner = await userManager.IsInRoleAsync(companyUser, assessorRole?.Name);
                if (isAssigner)
                {
                    currentOwner = companyUser.Email;
                    break;
                }
            }
            var lastLog = _context.InvestigationTransaction.Where(i =>
                i.ClaimsInvestigationId == claimsInvestigationId).OrderByDescending(o => o.Created)?.FirstOrDefault();

            claim.Updated = DateTime.UtcNow;
            claim.UpdatedBy = supervisorUser;
            claim.CurrentUserEmail = userEmail;
            claim.CurrentClaimOwner = currentOwner;
            claim.InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId;
            claim.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus
                .FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR).InvestigationCaseSubStatusId;

            var report = _context.ClaimReport.FirstOrDefault(c => c.ClaimReportId == caseLocation.ClaimReport.ClaimReportId);
            report.SupervisorRemarkType = reportUpdateStatus;
            report.SupervisorRemarks = supervisorRemarks;
            report.SupervisorRemarksUpdated = DateTime.UtcNow;
            report.SupervisorEmail = userEmail;
            report.Vendor = claim.Vendor;
            _context.ClaimReport.Update(report);
            caseLocation.ClaimReport = report;
            caseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus
                .FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR).InvestigationCaseSubStatusId;
            caseLocation.Updated = DateTime.UtcNow;
            caseLocation.UpdatedBy = userEmail;
            caseLocation.AssignedAgentUserEmail = string.Empty;
            _context.CaseLocation.Update(caseLocation);

            var lastLogHop = _context.InvestigationTransaction
                                       .Where(i => i.ClaimsInvestigationId == claimsInvestigationId)
                .AsNoTracking().Max(s => s.HopCount);

            var log = new InvestigationTransaction
            {
                HopCount = lastLogHop + 1,
                ClaimsInvestigationId = claimsInvestigationId,
                CurrentClaimOwner = currentOwner,
                Created = DateTime.UtcNow,
                Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR).InvestigationCaseSubStatusId,
                UpdatedBy = supervisorUser
            };
            _context.InvestigationTransaction.Add(log);
            _context.ClaimsInvestigation.Update(claim);
            try
            {
                return await _context.SaveChangesAsync() > 0 ? claim : null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<ClaimsInvestigation> ReAllocateToVendor(string userEmail, string claimsInvestigationId, long caseLocationId, string supervisorRemarks, SupervisorRemarkType reportUpdateStatus)
        {
            var claimsCaseLocation = _context.CaseLocation
            .Include(c => c.ClaimReport)
            .Include(c => c.ClaimsInvestigation)
            .Include(c => c.InvestigationCaseSubStatus)
            .Include(c => c.Vendor)
            .Include(c => c.PinCode)
            .Include(c => c.District)
            .Include(c => c.State)
            .Include(c => c.State)
            .FirstOrDefault(c => c.CaseLocationId == caseLocationId && c.ClaimsInvestigationId == claimsInvestigationId);

            var report = _context.ClaimReport.FirstOrDefault(c => c.ClaimReportId == claimsCaseLocation.ClaimReport.ClaimReportId);
            report.SupervisorRemarkType = reportUpdateStatus;
            report.SupervisorRemarks = supervisorRemarks;

            _context.ClaimReport.Update(report);
            claimsCaseLocation.ClaimReport = report;
            claimsCaseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                    i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR).InvestigationCaseSubStatusId;
            claimsCaseLocation.IsReviewCaseLocation = true;
            _context.CaseLocation.Update(claimsCaseLocation);

            var claimsCaseToAllocateToVendor = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .FirstOrDefault(v => v.ClaimsInvestigationId == claimsInvestigationId);
            claimsCaseToAllocateToVendor.Updated = DateTime.UtcNow;
            claimsCaseToAllocateToVendor.UpdatedBy = userEmail;
            claimsCaseToAllocateToVendor.CurrentUserEmail = userEmail;
            claimsCaseToAllocateToVendor.IsReviewCase = true;
            claimsCaseToAllocateToVendor.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                    i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR).InvestigationCaseSubStatusId;
            var existinCaseLocation = claimsCaseToAllocateToVendor.CaseLocations.FirstOrDefault(c => c.CaseLocationId == caseLocationId);
            existinCaseLocation.InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(
                    i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR).InvestigationCaseSubStatusId;
            _context.ClaimsInvestigation.Update(claimsCaseToAllocateToVendor);

            var lastLog = _context.InvestigationTransaction.Where(i =>
                 i.ClaimsInvestigationId == claimsInvestigationId).OrderByDescending(o => o.Created)?.FirstOrDefault();

            var lastLogHop = _context.InvestigationTransaction
                                        .Where(i => i.ClaimsInvestigationId == claimsInvestigationId)
                 .AsNoTracking().Max(s => s.HopCount);

            var log = new InvestigationTransaction
            {
                HopCount = lastLogHop + 1,
                ClaimsInvestigationId = claimsInvestigationId,
                Created = DateTime.UtcNow,
                Time2Update = DateTime.UtcNow.Subtract(lastLog.Created).Days,
                InvestigationCaseStatusId = _context.InvestigationCaseStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.INPROGRESS).InvestigationCaseStatusId,
                InvestigationCaseSubStatusId = _context.InvestigationCaseSubStatus.FirstOrDefault(i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR).InvestigationCaseSubStatusId,
                UpdatedBy = userEmail
            };
            _context.InvestigationTransaction.Add(log);

            return await _context.SaveChangesAsync() > 0 ? claimsCaseToAllocateToVendor : null;
        }

        public List<ClaimsInvestigation> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}