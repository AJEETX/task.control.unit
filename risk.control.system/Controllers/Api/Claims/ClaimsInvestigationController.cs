using System.Security.Claims;
using System.Net;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

using static risk.control.system.Helpers.Permissions;

using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Collections.Generic;
using Highsoft.Web.Mvc.Charts;
using risk.control.system.Services;

namespace risk.control.system.Controllers.Api.Claims
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsInvestigationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpClientService httpClientService;
        private static HttpClient httpClient = new HttpClient();

        public ClaimsInvestigationController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IHttpClientService httpClientService)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
            this.httpClientService = httpClientService;
        }

        [HttpGet("GetActive")]
        public async Task<IActionResult> GetActive()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
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
                .Where(c => !c.Deleted);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var clientCompany = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            if (clientCompany == null)
            {
            }
            else
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == clientCompany.ClientCompanyId);
            }
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var openStatuses = _context.InvestigationCaseStatus.Where(i => !i.Name.Contains(CONSTANTS.CASE_STATUS.FINISHED)).ToList();
            var assignedToAssignerStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var allocateToVendorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR);
            var assignedToAgentStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT);

            var submittededToSupervisorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR);

            var submittededToAssesssorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);

            var reAssigned2AssignerStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);

            var claimsSubmitted = new List<ClaimsInvestigation>();

            if (userRole.Value.Contains(AppRoles.Creator.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a => openStatusesIds.Contains(a.InvestigationCaseStatusId));
                claimsSubmitted = await applicationDbContext.ToListAsync();
            }
            else if (userRole.Value.Contains(AppRoles.Assigner.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a =>
                openStatusesIds.Contains(a.InvestigationCaseStatusId) && a.InvestigationCaseSubStatusId == assignedToAssignerStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == submittededToSupervisorStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == submittededToAssesssorStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == reAssigned2AssignerStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId);
                claimsSubmitted = await applicationDbContext.ToListAsync();
            }
            else if (userRole.Value.Contains(AppRoles.Assessor.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == submittededToAssesssorStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsSubmitted.Add(item);
                    }
                }
            }
            else if (userRole.Value.Contains(AppRoles.AgencyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Supervisor.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a =>
                openStatusesIds.Contains(a.InvestigationCaseStatusId) &&
                a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId ||
                a.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId ||
                a.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId
                );
                claimsSubmitted = await applicationDbContext.ToListAsync();
            }
            var response = claimsSubmitted
                    .Select(a => new
                    {
                        Id = a.ClaimsInvestigationId,
                        SelectedToAssign = false,
                        Agent = !string.IsNullOrWhiteSpace(a.CurrentClaimOwner) ?
                        string.Join("", "<span class='badge badge-light'>" + a.CurrentClaimOwner + "</span>") :
                        string.Join("", "<span class='badge badge-light'>" + a.UpdatedBy + "</span>"),
                        Pincode = GetPincode(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Document = a.PolicyDetail?.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail?.DocumentImage)) : "/img/no-policy.jpg",
                        Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail?.ProfilePicture)) : "/img/user.png",
                        Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"> <i class=\"fas fa-exclamation-triangle\" ></i>  </span>",
                        Policy = a.PolicyDetail?.LineOfBusiness.Name,
                        Status = a.InvestigationCaseStatus.Name,
                        SubStatus = a.InvestigationCaseSubStatus.Name,
                        Ready2Assign = a.IsReady2Assign,
                        ServiceType = a.PolicyDetail?.ClaimType.GetEnumDisplayName(),
                        Location = a.CaseLocations.Count == 0 ?
                        string.Join("", "<span class='badge badge-light'>" + a.InvestigationCaseSubStatus.Name + "</span>") :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                        Created = a.Created.ToString("dd-MM-yyyy"),
                        timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                        Withdrawable = a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId ? true : false,
                        PolicyNum = a.PolicyDetail.ContractNumber,
                        BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                        BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"> <i class=\"fas fa-exclamation-triangle\" ></i>  </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
                    })?
                    .ToList();

            return Ok(response);
        }

        private string GetPincode(ClaimType? claimType, CustomerDetail cdetail, CaseLocation location)
        {
            if (claimType == ClaimType.HEALTH)
            {
                if (cdetail is null)
                    return "<span class=\"badge badge-danger\"> <i class=\"fas fa-exclamation-triangle\" ></i>  </span>";
                return cdetail.PinCode.Code;
            }
            else
            {
                if (location is null)
                    return "<span class=\"badge badge-danger\"> <i class=\"fas fa-exclamation-triangle\" ></i>  </span>";
                return location.PinCode.Code;
            }
        }

        [HttpGet("GetActiveMap")]
        public async Task<IActionResult> GetActiveMap()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
                 .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
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
                .Where(c => !c.Deleted);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var clientCompany = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            if (clientCompany == null)
            {
            }
            else
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == clientCompany.ClientCompanyId);
            }
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var openStatuses = _context.InvestigationCaseStatus.Where(i => !i.Name.Contains(CONSTANTS.CASE_STATUS.FINISHED)).ToList();
            var assignedToAssignerStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var allocateToVendorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR);
            var assignedToAgentStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT);

            var submittededToSupervisorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR);

            var submittededToAssesssorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);

            var reAssigned2AssignerStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);

            var claimsSubmitted = new List<ClaimsInvestigation>();

            if (userRole.Value.Contains(AppRoles.Creator.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a => openStatusesIds.Contains(a.InvestigationCaseStatusId));
                claimsSubmitted = await applicationDbContext.ToListAsync();
            }
            else if (userRole.Value.Contains(AppRoles.Assigner.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a =>
                openStatusesIds.Contains(a.InvestigationCaseStatusId) && a.InvestigationCaseSubStatusId == assignedToAssignerStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == submittededToSupervisorStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == submittededToAssesssorStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == reAssigned2AssignerStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId);
                claimsSubmitted = await applicationDbContext.ToListAsync();
            }
            else if (userRole.Value.Contains(AppRoles.Assessor.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == submittededToAssesssorStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsSubmitted.Add(item);
                    }
                }
            }
            else if (userRole.Value.Contains(AppRoles.AgencyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Supervisor.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a =>
                openStatusesIds.Contains(a.InvestigationCaseStatusId) &&
                a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId ||
                a.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId ||
                a.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId
                );
                claimsSubmitted = await applicationDbContext.ToListAsync();
            }
            var company = _context.ClientCompany.Include(c => c.PinCode).FirstOrDefault(c => c.ClientCompanyId == clientCompany.ClientCompanyId);

            var response = claimsSubmitted
                   .Select(a => new MapResponse
                   {
                       Id = a.ClaimsInvestigationId,
                       Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                       Description = a.PolicyDetail.CauseOfLoss,
                       Price = a.PolicyDetail.SumAssuredValue,
                       Type = a.PolicyDetail.ClaimType == ClaimType.HEALTH ? "home" : "building",
                       Bed = a.CustomerDetail?.CustomerIncome.GetEnumDisplayName(),
                       Bath = a.CustomerDetail?.ContactNumber,
                       Size = a.CustomerDetail?.Description,
                       Position = new Position
                       {
                           Lat = GetLat(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()) ?? decimal.Parse(company.PinCode.Latitude),
                           Lng = GetLng(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()) ?? decimal.Parse(company.PinCode.Longitude),
                       },
                       Url = (a.CaseLocations?.FirstOrDefault() != null && a.CaseLocations?.FirstOrDefault().InvestigationCaseSubStatus.Code != CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR) ? "/ClaimsInvestigation/Detail?Id=" + a.ClaimsInvestigationId : "/ClaimsInvestigation/Details?Id=" + a.ClaimsInvestigationId
                   })?
                   .ToList();

            foreach (var item in response)
            {
                var isExist = response.Any(r => r.Position.Lng == item.Position.Lng && r.Position.Lat == item.Position.Lat && item.Id != r.Id);
                if (isExist)
                {
                    var (lat, lng) = LocationDetail.GetLatLng(item.Position.Lat, item.Position.Lng);
                    item.Position = new Position
                    {
                        Lat = lat,
                        Lng = lng,
                    };
                }
            }
            return Ok(new
            {
                response = response,
                lat = decimal.Parse(company.PinCode.Latitude),
                lng = decimal.Parse(company.PinCode.Longitude)
            });
        }

        [HttpGet("GetFtpData")]
        public List<string> GetFtpData()
        {
            var request = System.Net.WebRequest.Create(Applicationsettings.FTP_SITE);
            request.Method = System.Net.WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new System.Net.NetworkCredential(Applicationsettings.FTP_SITE_LOG, Applicationsettings.FTP_SITE_DATA);

            var files = new List<string>();

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, true))
                    {
                        while (!reader.EndOfStream)
                        {
                            var file = reader.ReadLine();
                            //Make sure you only get the filename and not the whole path.
                            file = file.Substring(file.LastIndexOf('/') + 1);
                            //The root folder will also be added, this can of course be ignored.
                            if (!file.StartsWith("."))
                                files.Add(file);
                        }
                    }
                }
            }
            var zipFiles = files.Where(f => f.EndsWith(".zip"));

            return zipFiles?.ToList();
        }

        private decimal? GetLat(ClaimType? claimType, CustomerDetail a, CaseLocation location)
        {
            if (claimType == ClaimType.HEALTH)
            {
                if (a is null)
                    return null;
                return decimal.Parse(a.PinCode.Latitude);
            }
            else
            {
                if (location is null)
                    return null;
                return decimal.Parse(location.PinCode.Latitude);
            }
        }

        private decimal? GetLng(ClaimType? claimType, CustomerDetail a, CaseLocation location)
        {
            if (claimType == ClaimType.HEALTH)
            {
                if (a is null)
                    return null;
                return decimal.Parse(a.PinCode.Longitude);
            }
            else
            {
                if (location is null)
                    return null;
                return decimal.Parse(location.PinCode.Longitude);
            }
        }

        [HttpGet("GetAssign")]
        public IActionResult GetAssign()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var reAssignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);
            var submittedToAssessorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }

            // SHOWING DIFFERRENT PAGES AS PER ROLES
            if (userRole.Value.Contains(AppRoles.PortalAdmin.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Creator.ToString()))
            {
                applicationDbContext = applicationDbContext
                    .Include(c => c.CaseLocations)
                    .ThenInclude(c => c.PinCode)
                    .Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => (c.VendorId == null && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId)
                    ) || a.IsReviewCase);

                var claimsAssigned = new List<ClaimsInvestigation>();
                foreach (var item in applicationDbContext)
                {
                    if (item.IsReady2Assign)
                    {
                        item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                        && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId || item.IsReviewCase)?.ToList();
                        if (item.CaseLocations.Any())
                        {
                            claimsAssigned.Add(item);
                        }
                    }
                }
                var response = claimsAssigned
                    .Select(a => new
                    {
                        Id = a.ClaimsInvestigationId,
                        SelectedToAssign = false,
                        Agent = !string.IsNullOrWhiteSpace(a.CurrentClaimOwner) ?
                        string.Join("", "<span class='badge badge-light'>" + a.CurrentClaimOwner + "</span>") :
                        string.Join("", "<span class='badge badge-light'>" + a.UpdatedBy + "</span>"),
                        Document = a.PolicyDetail?.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
                        Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
                        Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                        Policy = a.PolicyDetail?.LineOfBusiness?.Name,
                        Status = a.InvestigationCaseStatus.Name,
                        Pincode = GetPincode(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        ServiceType = a.PolicyDetail?.ClaimType?.GetEnumDisplayName(),
                        Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                        Created = a.Created.ToString("dd-MM-yyyy"),
                        timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                        PolicyNum = a.PolicyDetail.ContractNumber,
                        BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                        BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
                        Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations.FirstOrDefault()),
                        Description = a.PolicyDetail.CauseOfLoss,
                        Price = a.PolicyDetail.SumAssuredValue,
                        Type = a.PolicyDetail.ClaimType?.GetEnumDisplayName(),
                        Bed = a.CustomerDetail.CustomerIncome.GetEnumDisplayName(),
                        Bath = a.CustomerDetail.ContactNumber,
                        Size = a.CustomerDetail.Description,
                        Lat = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            a.CustomerDetail.PinCode.Latitude : a.CaseLocations.FirstOrDefault().PinCode.Latitude,
                        Long = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            a.CustomerDetail.PinCode.Longitude : a.CaseLocations.FirstOrDefault().PinCode.Longitude
                    })?
                    .ToList();

                return Ok(response);
            }
            return Ok(null);
        }

        [HttpGet("GetAssignMap")]
        public IActionResult GetAssignMap()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var submittedToAssessorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }

            // SHOWING DIFFERRENT PAGES AS PER ROLES
            if (userRole.Value.Contains(AppRoles.PortalAdmin.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Creator.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId)
                || a.IsReviewCase);

                var claimsAssigned = new List<ClaimsInvestigation>();
                foreach (var item in applicationDbContext)
                {
                    if (item.IsReady2Assign)
                    {
                        item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                        && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId || item.IsReviewCase)?.ToList();
                        if (item.CaseLocations.Any())
                        {
                            claimsAssigned.Add(item);
                        }
                    }
                }
                var response = claimsAssigned
                    .Select(a => new MapResponse
                    {
                        Id = a.ClaimsInvestigationId,
                        Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Description = a.PolicyDetail.CauseOfLoss,
                        Price = a.PolicyDetail.SumAssuredValue,
                        Type = a.PolicyDetail.ClaimType == ClaimType.HEALTH ? "home" : "building",
                        Bed = a.CustomerDetail.CustomerIncome.GetEnumDisplayName(),
                        Bath = a.CustomerDetail.ContactNumber,
                        Size = a.CustomerDetail.Description,
                        Position = new Position
                        {
                            Lat = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                           decimal.Parse(a.CustomerDetail.PinCode.Latitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Latitude),
                            Lng = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            decimal.Parse(a.CustomerDetail.PinCode.Longitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Longitude)
                        },
                        Url = (a.CaseLocations?.FirstOrDefault() != null && a.CaseLocations?.FirstOrDefault().InvestigationCaseSubStatus.Code != CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR) ? "/ClaimsInvestigation/Detail?Id=" + a.ClaimsInvestigationId : "/ClaimsInvestigation/Details?Id=" + a.ClaimsInvestigationId
                    })?
                    .ToList();
                var company = _context.ClientCompany.Include(c => c.PinCode).FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

                foreach (var item in response)
                {
                    var isExist = response.Any(r => r.Position.Lng == item.Position.Lng && r.Position.Lat == item.Position.Lat && item.Id != r.Id);
                    if (isExist)
                    {
                        var (lat, lng) = LocationDetail.GetLatLng(item.Position.Lat, item.Position.Lng);
                        item.Position = new Position
                        {
                            Lat = lat,
                            Lng = lng,
                        };
                    }
                }

                return Ok(new
                {
                    response = response,
                    lat = decimal.Parse(company.PinCode.Latitude),
                    lng = decimal.Parse(company.PinCode.Longitude)
                });
            }
            return Problem();
        }

        [HttpGet("GetIncomplete")]
        public async Task<IActionResult> GetIncomplete()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
.Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);
            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);

            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser != null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId && i.VendorId == vendorUser.VendorId);
            }

            // SHOWING DIFFERRENT PAGES AS PER ROLES
            if (userRole.Value.Contains(AppRoles.PortalAdmin.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Creator.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId && !a.IsReady2Assign);
                var claims = await applicationDbContext.ToListAsync();
                try
                {
                    var response = claims
                        .Select(a => new
                        {
                            Id = a.ClaimsInvestigationId,
                            SelectedToAssign = false,
                            Document = a.PolicyDetail.DocumentImage != null ?
                            string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) :
                            "/img/no-policy.jpg",
                            Customer = (a.CustomerDetail != null && a.CustomerDetail.ProfilePicture != null) ?
                            string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) :
                            "/img/user.png",
                            Name = a.CustomerDetail != null ? a.CustomerDetail.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                            Policy = a.PolicyDetail.LineOfBusiness.Name,
                            Status = a.InvestigationCaseStatus.Name,
                            ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
                            Location = a.CaseLocations.Count == 0 ?
                            "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                            string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                            Created = a.Created.ToString("dd-MM-yyyy"),
                            PolicyNum = a.PolicyDetail.ContractNumber,
                            BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                            BeneficiaryName = a.CaseLocations.Count == 0 ?
                            "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                            a.CaseLocations.FirstOrDefault().BeneficiaryName,
                            timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString()
                        })?.ToList();

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Ok(await applicationDbContext.ToListAsync());
        }

        [HttpGet("GetAssigner")]
        public IActionResult GetAssigner()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var submittedToAssessorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }

            // SHOWING DIFFERRENT PAGES AS PER ROLES
            applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null));

            var claimsAssigned = new List<ClaimsInvestigation>();

            foreach (var item in applicationDbContext)
            {
                item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                    && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId)?.ToList();
                if (item.CaseLocations.Any())
                {
                    claimsAssigned.Add(item);
                }
            }
            var response = claimsAssigned
                    .Select(a => new
                    {
                        Id = a.ClaimsInvestigationId,
                        SelectedToAssign = false,
                        Agent = !string.IsNullOrWhiteSpace(a.CurrentClaimOwner) ?
                        string.Join("", "<span class='badge badge-light'>" + a.CurrentClaimOwner + "</span>") :
                        string.Join("", "<span class='badge badge-light'>" + a.UpdatedBy + "</span>"),
                        Pincode = GetPincode(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Document = a.PolicyDetail.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
                        Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
                        Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                        Policy = a.PolicyDetail.LineOfBusiness.Name,
                        Status = a.InvestigationCaseStatus.Name,
                        ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
                        Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                        Created = a.Created.ToString("dd-MM-yyyy"),
                        timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                        PolicyNum = a.PolicyDetail.ContractNumber,
                        BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                        BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
                    })
                    ?.ToList();

            return Ok(response);
        }

        [HttpGet("GetAssignerMap")]
        public IActionResult GetAssignerMap()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var submittedToAssessorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }

            // SHOWING DIFFERRENT PAGES AS PER ROLES
            applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null));

            var claimsAssigned = new List<ClaimsInvestigation>();

            foreach (var item in applicationDbContext)
            {
                item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                    && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId)?.ToList();
                if (item.CaseLocations.Any())
                {
                    claimsAssigned.Add(item);
                }
            }
            var response = claimsAssigned
                    .Select(a => new MapResponse
                    {
                        Id = a.ClaimsInvestigationId,
                        Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Description = a.PolicyDetail.CauseOfLoss,
                        Price = a.PolicyDetail.SumAssuredValue,
                        Type = a.PolicyDetail.ClaimType == ClaimType.HEALTH ? "home" : "building",
                        Bed = a.CustomerDetail.CustomerIncome.GetEnumDisplayName(),
                        Bath = a.CustomerDetail.ContactNumber,
                        Size = a.CustomerDetail.Description,
                        Position = new Position
                        {
                            Lat = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                           decimal.Parse(a.CustomerDetail.PinCode.Latitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Latitude),
                            Lng = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            decimal.Parse(a.CustomerDetail.PinCode.Longitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Longitude)
                        },
                        Url = (a.CaseLocations?.FirstOrDefault() != null && a.CaseLocations?.FirstOrDefault().InvestigationCaseSubStatus.Code != CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR) ? "/ClaimsInvestigation/Detail?Id=" + a.ClaimsInvestigationId : "/ClaimsInvestigation/Details?Id=" + a.ClaimsInvestigationId
                    })?
                    .ToList();

            var company = _context.ClientCompany.Include(c => c.PinCode).FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            foreach (var item in response)
            {
                var isExist = response.Any(r => r.Position.Lng == item.Position.Lng && r.Position.Lat == item.Position.Lat && item.Id != r.Id);
                if (isExist)
                {
                    var (lat, lng) = LocationDetail.GetLatLng(item.Position.Lat, item.Position.Lng);
                    item.Position = new Position
                    {
                        Lat = lat,
                        Lng = lng,
                    };
                }
            }
            return Ok(new
            {
                response = response,
                lat = decimal.Parse(company.PinCode.Latitude),
                lng = decimal.Parse(company.PinCode.Longitude)
            });
        }

        [HttpGet("GetAssessor")]
        public IActionResult GetAssessor()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var submittedToAssessorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }

            var claimsAssigned = new List<ClaimsInvestigation>();
            if (userRole.Value.Contains(AppRoles.PortalAdmin.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Creator.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                    && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsAssigned.Add(item);
                    }
                }
            }
            else if (userRole.Value.Contains(AppRoles.Assigner.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                        && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsAssigned.Add(item);
                    }
                }
            }
            else if (userRole.Value.Contains(AppRoles.Assessor.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == submittedToAssessorStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsAssigned.Add(item);
                    }
                }
            }

            var response = claimsAssigned
            .Select(a => new
            {
                Id = a.ClaimsInvestigationId,
                SelectedToAssign = false,
                Pincode = GetPincode(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                Document = a.PolicyDetail.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
                Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
                Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                Policy = a.PolicyDetail.LineOfBusiness.Name,
                Status = a.InvestigationCaseStatus.Name,
                ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
                Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                Created = a.Created.ToString("dd-MM-yyyy"),
                timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                PolicyNum = a.PolicyDetail.ContractNumber,
                BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
            })
            ?.ToList();

            return Ok(response);
        }

        [HttpGet("GetAssessorMap")]
        public IActionResult GetAssessorMap()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var submittedToAssessorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }

            var claimsAssigned = new List<ClaimsInvestigation>();
            if (userRole.Value.Contains(AppRoles.PortalAdmin.ToString()) || userRole.Value.Contains(AppRoles.CompanyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Creator.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                    && c.InvestigationCaseSubStatusId == createdStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsAssigned.Add(item);
                    }
                }
            }
            else if (userRole.Value.Contains(AppRoles.Assigner.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId == null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => string.IsNullOrWhiteSpace(c.VendorId)
                        && c.InvestigationCaseSubStatusId == assignedStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsAssigned.Add(item);
                    }
                }
            }
            else if (userRole.Value.Contains(AppRoles.Assessor.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == submittedToAssessorStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsAssigned.Add(item);
                    }
                }
            }

            var response = claimsAssigned
                    .Select(a => new MapResponse
                    {
                        Id = a.ClaimsInvestigationId,
                        Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Description = a.PolicyDetail.CauseOfLoss,
                        Price = a.PolicyDetail.SumAssuredValue,
                        Type = a.PolicyDetail.ClaimType == ClaimType.HEALTH ? "home" : "building",
                        Bed = a.CustomerDetail.CustomerIncome.GetEnumDisplayName(),
                        Bath = a.CustomerDetail.ContactNumber,
                        Size = a.CustomerDetail.Description,
                        Position = new Position
                        {
                            Lat = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                           decimal.Parse(a.CustomerDetail.PinCode.Latitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Latitude),
                            Lng = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            decimal.Parse(a.CustomerDetail.PinCode.Longitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Longitude)
                        },
                        Url = (a.CaseLocations?.FirstOrDefault() != null && a.CaseLocations?.FirstOrDefault().InvestigationCaseSubStatus.Code != CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR) ? "/ClaimsInvestigation/Detail?Id=" + a.ClaimsInvestigationId : "/ClaimsInvestigation/Details?Id=" + a.ClaimsInvestigationId
                    })?
                    .ToList();
            var company = _context.ClientCompany.Include(c => c.PinCode).FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            foreach (var item in response)
            {
                var isExist = response.Any(r => r.Position.Lng == item.Position.Lng && r.Position.Lat == item.Position.Lat && item.Id != r.Id);
                if (isExist)
                {
                    var (lat, lng) = LocationDetail.GetLatLng(item.Position.Lat, item.Position.Lng);
                    item.Position = new Position
                    {
                        Lat = lat,
                        Lng = lng,
                    };
                }
            }
            return Ok(new
            {
                response = response,
                lat = decimal.Parse(company.PinCode.Latitude),
                lng = decimal.Parse(company.PinCode.Longitude)
            });
        }

        [HttpGet("GetApproved")]
        public IActionResult GetApproved()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);
            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var assessorApprovedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR);
            var reassignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            var claimsSubmitted = new List<ClaimsInvestigation>();

            if (userRole.Value.Contains(AppRoles.Assessor.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == assessorApprovedStatus.InvestigationCaseSubStatusId
                    || c.InvestigationCaseSubStatusId == reassignedStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsSubmitted.Add(item);
                    }
                }
            }
            var response = claimsSubmitted
            .Select(a => new
            {
                Id = a.ClaimsInvestigationId,
                SelectedToAssign = false,
                Pincode = GetPincode(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                Document = a.PolicyDetail.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
                Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
                Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                Policy = a.PolicyDetail.LineOfBusiness.Name,
                Status = a.InvestigationCaseStatus.Name,
                ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
                Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                Created = a.Created.ToString("dd-MM-yyyy"),
                timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                PolicyNum = a.PolicyDetail.ContractNumber,
                BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
            })
            ?.ToList();

            return Ok(response);
        }

        [HttpGet("GetApprovedMap")]
        public IActionResult GetApprovedMap()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);
            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var assessorApprovedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR);
            var reassignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            var claimsSubmitted = new List<ClaimsInvestigation>();

            if (userRole.Value.Contains(AppRoles.Assessor.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == assessorApprovedStatus.InvestigationCaseSubStatusId
                    || c.InvestigationCaseSubStatusId == reassignedStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsSubmitted.Add(item);
                    }
                }
            }
            var response = claimsSubmitted
                    .Select(a => new MapResponse
                    {
                        Id = a.ClaimsInvestigationId,
                        Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Description = a.PolicyDetail.CauseOfLoss,
                        Price = a.PolicyDetail.SumAssuredValue,
                        Type = a.PolicyDetail.ClaimType == ClaimType.HEALTH ? "home" : "building",
                        Bed = a.CustomerDetail.CustomerIncome.GetEnumDisplayName(),
                        Bath = a.CustomerDetail.ContactNumber,
                        Size = a.CustomerDetail.Description,
                        Position = new Position
                        {
                            Lat = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                           decimal.Parse(a.CustomerDetail.PinCode.Latitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Latitude),
                            Lng = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            decimal.Parse(a.CustomerDetail.PinCode.Longitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Longitude)
                        },
                        Url = (a.CaseLocations?.FirstOrDefault() != null && a.CaseLocations?.FirstOrDefault().InvestigationCaseSubStatus.Code != CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR) ? "/ClaimsInvestigation/Detail?Id=" + a.ClaimsInvestigationId : "/ClaimsInvestigation/Details?Id=" + a.ClaimsInvestigationId
                    })?
                    .ToList();
            var company = _context.ClientCompany.Include(c => c.PinCode).FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            foreach (var item in response)
            {
                var isExist = response.Any(r => r.Position.Lng == item.Position.Lng && r.Position.Lat == item.Position.Lat && item.Id != r.Id);
                if (isExist)
                {
                    var (lat, lng) = LocationDetail.GetLatLng(item.Position.Lat, item.Position.Lng);
                    item.Position = new Position
                    {
                        Lat = lat,
                        Lng = lng,
                    };
                }
            }
            return Ok(new
            {
                response = response,
                lat = decimal.Parse(company.PinCode.Latitude),
                lng = decimal.Parse(company.PinCode.Longitude)
            });
        }

        [HttpGet("GetReview")]
        public IActionResult GetReview()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.ClientCompany)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CaseEnabler)
                .Include(c => c.PolicyDetail)
                .ThenInclude(c => c.CostCentre)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.InvestigationCaseSubStatus)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.PinCode)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);

            var createdStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR);
            var assignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var reassignedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(i =>
                i.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.REASSIGNED_TO_ASSIGNER);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);

            if (companyUser == null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            else if (companyUser != null && vendorUser == null)
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId);
            }
            var claimsSubmitted = new List<ClaimsInvestigation>();

            if (userRole.Value.Contains(AppRoles.Assessor.ToString()) || userRole.Value.Contains(AppRoles.Assigner.ToString()))
            {
                applicationDbContext = applicationDbContext.Where(a => a.CaseLocations.Count > 0 && a.CaseLocations.Any(c => c.VendorId != null));

                foreach (var item in applicationDbContext)
                {
                    item.CaseLocations = item.CaseLocations.Where(c => c.InvestigationCaseSubStatusId == reassignedStatus.InvestigationCaseSubStatusId)?.ToList();
                    if (item.CaseLocations.Any())
                    {
                        claimsSubmitted.Add(item);
                    }
                }
            }

            var response = claimsSubmitted
        .Select(a => new
        {
            Id = a.ClaimsInvestigationId,
            SelectedToAssign = false,
            Document = a.PolicyDetail.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
            Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
            Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
            Policy = a.PolicyDetail.LineOfBusiness.Name,
            Status = a.InvestigationCaseStatus.Name,
            ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
            Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
            Created = a.Created.ToString("dd-MM-yyyy"),
            timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
            PolicyNum = a.PolicyDetail.ContractNumber,
            BeneficiaryPhoto = a.CaseLocations.Count == 0 ?
                        "/img/user.png" :
                        string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)),
            BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
        })
        ?.ToList();

            return Ok(response);
        }

        [HttpGet("GetToInvestigate")]
        public async Task<IActionResult> GetToInvestigate()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted);
            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var openStatuses = _context.InvestigationCaseStatus.Where(i => !i.Name.Contains(CONSTANTS.CASE_STATUS.FINISHED)).ToList();
            var assignedToAssignerStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_ASSIGNER);
            var allocateToVendorStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR);

            if (userRole.Value.Contains(AppRoles.Creator.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a => openStatusesIds.Contains(a.InvestigationCaseStatusId));
            }
            else if (userRole.Value.Contains(AppRoles.Assigner.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a =>
                openStatusesIds.Contains(a.InvestigationCaseStatusId) && a.InvestigationCaseSubStatusId == assignedToAssignerStatus.InvestigationCaseSubStatusId
                || a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId);
            }
            else if (userRole.Value.Contains(AppRoles.AgencyAdmin.ToString()) || userRole.Value.Contains(AppRoles.Supervisor.ToString()))
            {
                var openStatusesIds = openStatuses.Select(i => i.InvestigationCaseStatusId).ToList();
                applicationDbContext = applicationDbContext.Where(a =>
                openStatusesIds.Contains(a.InvestigationCaseStatusId) && a.InvestigationCaseSubStatusId == allocateToVendorStatus.InvestigationCaseSubStatusId);
            }
            var userEmail = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            var clientCompany = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail.Value);
            if (clientCompany == null)
            {
            }
            else
            {
                applicationDbContext = applicationDbContext.Where(i => i.PolicyDetail.ClientCompanyId == clientCompany.ClientCompanyId);
            }
            var claimsSubmitted = await applicationDbContext.ToListAsync();
            var response = claimsSubmitted
                .Select(a => new
                {
                    Id = a.ClaimsInvestigationId,
                    SelectedToAssign = false,
                    Document = a.PolicyDetail.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
                    Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
                    Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                    Policy = a.PolicyDetail.LineOfBusiness.Name,
                    Status = a.InvestigationCaseStatus.Name,
                    ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
                    Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                    Created = a.Created.ToString("dd-MM-yyyy"),
                    timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                    PolicyNum = a.PolicyDetail.ContractNumber,
                    BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                    BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
                })
                ?.ToList();

            return Ok(response);
        }

        [HttpGet("GetReport")]
        public async Task<IActionResult> GetReport()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
               .Include(c => c.Vendor)
                .Where(c => !c.Deleted &&
                c.CustomerDetail != null && c.CaseLocations.Count > 0 &&
                c.CaseLocations.All(c => c.ClaimReport != null));
            var user = HttpContext.User.Identity.Name;

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(u => u.Email == user);

            var claimsSubmitted = await applicationDbContext.Where(c => c.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId &&
                (c.InvestigationCaseSubStatus.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR
                ))
                 .ToListAsync();

            var response =
                claimsSubmitted
            .Select(a => new
            {
                Id = a.ClaimsInvestigationId,
                SelectedToAssign = false,
                Agent = !string.IsNullOrWhiteSpace(a.CurrentClaimOwner) ?
                        string.Join("", "<span class='badge badge-light'>" + a.CurrentClaimOwner + "</span>") :
                        string.Join("", "<span class='badge badge-light'>" + a.UpdatedBy + "</span>"),
                Pincode = GetPincode(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                Document = a.PolicyDetail?.DocumentImage != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.PolicyDetail.DocumentImage)) : "/img/no-policy.jpg",
                Customer = a.CustomerDetail?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CustomerDetail.ProfilePicture)) : "/img/user.png",
                Name = a.CustomerDetail?.CustomerName != null ? a.CustomerDetail?.CustomerName : "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/user.png\" /> </span>",
                Policy = a.PolicyDetail.LineOfBusiness.Name,
                Status = a.InvestigationCaseStatus.Name,
                ServiceType = a.PolicyDetail.ClaimType.GetEnumDisplayName(),
                Location = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        string.Join("", a.CaseLocations.Select(c => "<span class='badge badge-light'>" + c.InvestigationCaseSubStatus.Name + "</span> ")),
                Created = a.Created.ToString("dd-MM-yyyy"),
                timePending = DateTime.Now.Subtract(a.Created).Days == 0 ? "< 1" : DateTime.Now.Subtract(a.Created).Days.ToString(),
                PolicyNum = a.PolicyDetail.ContractNumber,
                BeneficiaryPhoto = a.CaseLocations.Count != 0 && a.CaseLocations.FirstOrDefault().ProfilePicture != null ?
                                       string.Format("data:image/*;base64,{0}", Convert.ToBase64String(a.CaseLocations.FirstOrDefault().ProfilePicture)) :
                                      "/img/user.png",
                BeneficiaryName = a.CaseLocations.Count == 0 ?
                        "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                        a.CaseLocations.FirstOrDefault().BeneficiaryName,
                Agency = a.Vendor?.Name
            })
            ?.ToList();

            return Ok(response);
        }

        [HttpGet("GetReportMap")]
        public async Task<IActionResult> GetReportMap()
        {
            IQueryable<ClaimsInvestigation> applicationDbContext = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
                .Where(c => !c.Deleted &&
                c.CustomerDetail != null && c.CaseLocations.Count > 0 &&
                c.CaseLocations.All(c => c.ClaimReport != null));
            var user = HttpContext.User.Identity.Name;

            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(u => u.Email == user);

            var claimsSubmitted = await applicationDbContext.Where(c => c.PolicyDetail.ClientCompanyId == companyUser.ClientCompanyId &&
                c.InvestigationCaseSubStatus.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.APPROVED_BY_ASSESSOR ||
                c.InvestigationCaseSubStatus.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_ASSESSOR ||
                c.InvestigationCaseSubStatus.Name == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR
                )
                 .ToListAsync();

            var response = claimsSubmitted
                    .Select(a => new MapResponse
                    {
                        Id = a.ClaimsInvestigationId,
                        Address = LocationDetail.GetAddress(a.PolicyDetail.ClaimType, a.CustomerDetail, a.CaseLocations?.FirstOrDefault()),
                        Description = a.PolicyDetail.CauseOfLoss,
                        Price = a.PolicyDetail.SumAssuredValue,
                        Type = a.PolicyDetail.ClaimType == ClaimType.HEALTH ? "home" : "building",
                        Bed = a.CustomerDetail.CustomerIncome.GetEnumDisplayName(),
                        Bath = a.CustomerDetail.ContactNumber,
                        Size = a.CustomerDetail.Description,
                        Position = new Position
                        {
                            Lat = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                           decimal.Parse(a.CustomerDetail.PinCode.Latitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Latitude),
                            Lng = a.PolicyDetail.ClaimType == ClaimType.HEALTH ?
                            decimal.Parse(a.CustomerDetail.PinCode.Longitude) : decimal.Parse(a.CaseLocations.FirstOrDefault().PinCode.Longitude)
                        },
                        Url = (a.CaseLocations?.FirstOrDefault() != null && a.CaseLocations?.FirstOrDefault().InvestigationCaseSubStatus.Code != CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.CREATED_BY_CREATOR) ? "/ClaimsInvestigation/Detail?Id=" + a.ClaimsInvestigationId : "/ClaimsInvestigation/Details?Id=" + a.ClaimsInvestigationId
                    })?
                    .ToList();

            var company = _context.ClientCompany.Include(c => c.PinCode).FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            foreach (var item in response)
            {
                var isExist = response.Any(r => r.Position.Lng == item.Position.Lng && r.Position.Lat == item.Position.Lat && item.Id != r.Id);
                if (isExist)
                {
                    var (lat, lng) = LocationDetail.GetLatLng(item.Position.Lat, item.Position.Lng);
                    item.Position = new Position
                    {
                        Lat = lat,
                        Lng = lng,
                    };
                }
            }
            return Ok(new
            {
                response = response,
                lat = decimal.Parse(company.PinCode.Latitude),
                lng = decimal.Parse(company.PinCode.Longitude)
            });
        }

        [HttpGet("GetPolicyDetail")]
        public async Task<IActionResult> GetPolicyDetail(string id)
        {
            var policy = await _context.PolicyDetail
                .Include(p => p.LineOfBusiness)
                .Include(p => p.InvestigationServiceType)
                .Include(p => p.CostCentre)
                .Include(p => p.CaseEnabler)
                .FirstOrDefaultAsync(p => p.PolicyDetailId == id);

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "no-policy.jpg");

            var noDataimage = await System.IO.File.ReadAllBytesAsync(noDataImagefilePath);

            var response = new
            {
                Document =
                    policy.DocumentImage != null ?
                    string.Format("data:image/*;base64,{0}", Convert.ToBase64String(policy.DocumentImage)) :
                    string.Format("data:image/*;base64,{0}", Convert.ToBase64String(noDataimage)),
                ContractNumber = policy.ContractNumber,
                ClaimType = policy.ClaimType.GetEnumDisplayName(),
                ContractIssueDate = policy.ContractIssueDate.ToString("dd-MMM-yyyy"),
                DateOfIncident = policy.DateOfIncident.ToString("dd-MMM-yyyy"),
                SumAssuredValue = policy.SumAssuredValue,
                InvestigationServiceType = policy.InvestigationServiceType.Name,
                CaseEnabler = policy.CaseEnabler.Name,
                CauseOfLoss = policy.CauseOfLoss,
                CostCentre = policy.CostCentre.Name,
            };
            return Ok(response);
        }

        [HttpGet("GetCustomerDetail")]
        public async Task<IActionResult> GetCustomerDetail(string id)
        {
            var customer = await _context.CustomerDetail
                .Include(c => c.Country)
                .Include(c => c.State)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .FirstOrDefaultAsync(p => p.CustomerDetailId == id);

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "user.png");

            var noDataimage = await System.IO.File.ReadAllBytesAsync(noDataImagefilePath);

            return Ok(
                new
                {
                    Customer = customer?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(customer.ProfilePicture)) :
                    string.Format("data:image/*;base64,{0}", Convert.ToBase64String(noDataimage)),
                    CustomerName = customer.CustomerName,
                    ContactNumber = customer.ContactNumber,
                    Address = customer.Addressline + "  " + customer.District.Name + "  " + customer.State.Name + "  " + customer.Country.Name + "  " + customer.PinCode.Code,
                    Occupation = customer.CustomerOccupation.GetEnumDisplayName(),
                    Income = customer.CustomerIncome.GetEnumDisplayName(),
                    Education = customer.CustomerEducation.GetEnumDisplayName(),
                    DateOfBirth = customer.CustomerDateOfBirth
                }
                );
        }

        [HttpGet("GetBeneficiaryDetail")]
        public async Task<IActionResult> GetBeneficiaryDetail(long id, string claimId)
        {
            var beneficiary = await _context.CaseLocation
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.Country)
                .Include(c => c.State)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .Include(c => c.ClaimReport)
                .FirstOrDefaultAsync(p => p.CaseLocationId == id && p.ClaimsInvestigationId == claimId);

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "user.png");

            var noDataimage = await System.IO.File.ReadAllBytesAsync(noDataImagefilePath);

            return Ok(new
            {
                Beneficiary = beneficiary?.ProfilePicture != null ? string.Format("data:image/*;base64,{0}", Convert.ToBase64String(beneficiary.ProfilePicture)) :
                    string.Format("data:image/*;base64,{0}", Convert.ToBase64String(noDataimage)),
                BeneficiaryName = beneficiary.BeneficiaryName,
                Dob = (int)beneficiary.BeneficiaryDateOfBirth.Subtract(DateTime.Now).TotalDays / 365,
                Income = beneficiary.BeneficiaryIncome.GetEnumDisplayName(),
                BeneficiaryRelation = beneficiary.BeneficiaryRelation.Name,
                Address = beneficiary.Addressline + "  " + beneficiary.District.Name + "  " + beneficiary.State.Name + "  " + beneficiary.Country.Name + "  " + beneficiary.PinCode.Code,
                ContactNumber = beneficiary.BeneficiaryContactNumber
            }
            );
        }

        [HttpGet("GetInvestigationData")]
        public async Task<IActionResult> GetInvestigationData(long id, string claimId)
        {
            var beneficiary = await _context.CaseLocation
                .Include(c => c.ClaimReport)
                .FirstOrDefaultAsync(p => p.CaseLocationId == id && p.ClaimsInvestigationId == claimId);

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "no-photo.jpg");

            var noDataimage = await System.IO.File.ReadAllBytesAsync(noDataImagefilePath);

            string mapUrl = "https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=150x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            string imageAddress = string.Empty;
            string faceLat = string.Empty, faceLng = string.Empty;
            if (!string.IsNullOrWhiteSpace(beneficiary.ClaimReport?.LocationLongLat))
            {
                var longLat = beneficiary.ClaimReport.LocationLongLat.IndexOf("/");
                faceLat = beneficiary.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                faceLng = beneficiary.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim();
                var longLatString = faceLat + "," + faceLng;
                RootObject rootObject = await httpClientService.GetAddress((faceLat), (faceLng));
                imageAddress = rootObject.display_name;
                mapUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={longLatString}&zoom=18&size=300x300&maptype=roadmap&markers=color:red%7Clabel:S%7C{longLatString}&key={Applicationsettings.GMAPData}";
            }

            string ocrUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=150x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            string ocrAddress = string.Empty;
            string ocrLatitude = string.Empty, ocrLongitude = string.Empty;
            if (!string.IsNullOrWhiteSpace(beneficiary.ClaimReport?.OcrLongLat))
            {
                var ocrlongLat = beneficiary.ClaimReport.OcrLongLat.IndexOf("/");
                ocrLatitude = beneficiary.ClaimReport.OcrLongLat.Substring(0, ocrlongLat)?.Trim();
                ocrLongitude = beneficiary.ClaimReport.OcrLongLat.Substring(ocrlongLat + 1)?.Trim();
                var ocrLongLatString = ocrLatitude + "," + ocrLongitude;
                RootObject rootObject = await httpClientService.GetAddress((ocrLatitude), (ocrLongitude));
                ocrAddress = rootObject.display_name;
                ocrUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={ocrLongLatString}&zoom=18&size=300x300&maptype=roadmap&markers=color:red%7Clabel:S%7C{ocrLongLatString}&key={Applicationsettings.GMAPData}";
            }

            var data = new
            {
                Title = "Investigation Data",
                QrData = beneficiary.ClaimReport?.AgentOcrData,
                LocationData = beneficiary.ClaimReport?.LocationData ?? "Location Data",
                LatLong = mapUrl,
                ImageAddress = imageAddress,
                Location = beneficiary.ClaimReport?.AgentLocationPicture != null ?
                string.Format("data:image/*;base64,{0}", Convert.ToBase64String(beneficiary.ClaimReport?.AgentLocationPicture)) :
                string.Format("data:image/*;base64,{0}", Convert.ToBase64String(noDataimage)),
                OcrData = beneficiary.ClaimReport?.AgentOcrPicture != null ?
                string.Format("data:image/*;base64,{0}", Convert.ToBase64String(beneficiary.ClaimReport?.AgentOcrPicture)) :
                string.Format("data:image/*;base64,{0}", Convert.ToBase64String(noDataimage)),
                OcrLatLong = ocrUrl,
                OcrAddress = ocrAddress,
                FacePosition = new { Lat = decimal.Parse(faceLat), Lng = decimal.Parse(faceLng) },
                OcrPosition = new { Lat = decimal.Parse(ocrLatitude), Lng = decimal.Parse(ocrLongitude) }
            };

            return Ok(data);
        }

        [HttpGet("GetCustomerMap")]
        public async Task<IActionResult> GetCustomerMap(string id)
        {
            var customer = await _context.CustomerDetail
                .Include(c => c.Country)
                .Include(c => c.State)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .FirstOrDefaultAsync(p => p.CustomerDetailId == id);

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "no-image.png");

            var noDataimage = await System.IO.File.ReadAllBytesAsync(noDataImagefilePath);

            var latitude = customer.PinCode.Latitude;
            var longitude = customer.PinCode.Longitude.Trim();
            var latLongString = latitude + "," + longitude;
            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,windspeed_10m&hourly=temperature_2m,relativehumidity_2m,windspeed_10m";
            var weatherData = await httpClient.GetFromJsonAsync<Weather>(weatherUrl);
            string weatherCustomData = $"Temperature:{weatherData.current.temperature_2m} {weatherData.current_units.temperature_2m}.\r\nWindspeed:{weatherData.current.windspeed_10m} {weatherData.current_units.windspeed_10m} \r\nElevation(sea level):{weatherData.elevation} metres";

            var longLatString = latitude + "," + longitude;
            RootObject rootObject = await httpClientService.GetAddress((latitude), (longitude));
            var imageAddress = rootObject.display_name;
            var customerMapUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={longLatString}&zoom=18&size=300x300&maptype=roadmap&markers=color:red%7Clabel:S%7C{longLatString}&key={Applicationsettings.GMAPData}";
            var data = new
            {
                profileMap = customerMapUrl,
                weatherData = weatherCustomData,
                address = customer.Addressline + " " + customer.District.Name + " " + customer.State.Name + " " + customer.Country.Name + " " + customer.PinCode.Code,
                position = new { Lat = decimal.Parse(latitude), Lng = decimal.Parse(longitude) }
            };
            return Ok(data);
        }

        [HttpGet("GetBeneficiaryMap")]
        public async Task<IActionResult> GetBeneficiaryMap(long id, string claimId)
        {
            var beneficiary = await _context.CaseLocation
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.Country)
                .Include(c => c.State)
                .Include(c => c.District)
                .Include(c => c.PinCode)
            .Include(c => c.ClaimReport)
                .FirstOrDefaultAsync(p => p.CaseLocationId == id && p.ClaimsInvestigationId == claimId);

            var noDataImagefilePath = Path.Combine(webHostEnvironment.WebRootPath, "img", "no-image.png");

            var noDataimage = await System.IO.File.ReadAllBytesAsync(noDataImagefilePath);

            var latitude = beneficiary.PinCode.Latitude;
            var longitude = beneficiary.PinCode.Longitude.Trim();
            var latLongString = latitude + "," + longitude;
            var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,windspeed_10m&hourly=temperature_2m,relativehumidity_2m,windspeed_10m";
            var weatherData = await httpClient.GetFromJsonAsync<Weather>(weatherUrl);
            string weatherCustomData = $"Temperature:{weatherData.current.temperature_2m} {weatherData.current_units.temperature_2m}.\r\nWindspeed:{weatherData.current.windspeed_10m} {weatherData.current_units.windspeed_10m} \r\nElevation(sea level):{weatherData.elevation} metres";

            var longLatString = latitude + "," + longitude;
            RootObject rootObject = await httpClientService.GetAddress((latitude), (longitude));
            var imageAddress = rootObject.display_name;
            var customerMapUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={longLatString}&zoom=18&size=300x300&maptype=roadmap&markers=color:red%7Clabel:S%7C{longLatString}&key={Applicationsettings.GMAPData}";
            var data = new
            {
                profileMap = customerMapUrl,
                weatherData = weatherCustomData,
                address = beneficiary.Addressline + " " + beneficiary.District.Name + " " + beneficiary.State.Name + " " + beneficiary.Country.Name + " " + beneficiary.PinCode.Code,
                position = new { Lat = decimal.Parse(latitude), Lng = decimal.Parse(longitude) }
            };
            return Ok(data);
        }

        [HttpGet("GetFaceDetail")]
        public IActionResult GetFaceDetail(string claimid)
        {
            var claim = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
               .Include(c => c.CaseLocations)
               .ThenInclude(l => l.ClaimReport)
                .FirstOrDefault(c => c.ClaimsInvestigationId == claimid);

            if (claim.PolicyDetail.ClaimType == ClaimType.HEALTH)
            {
                var center = new { Lat = decimal.Parse(claim.CustomerDetail.PinCode.Latitude), Lng = decimal.Parse(claim.CustomerDetail.PinCode.Longitude) };
                var dakota = new { Lat = decimal.Parse(claim.CustomerDetail.PinCode.Latitude), Lng = decimal.Parse(claim.CustomerDetail.PinCode.Longitude) };

                if (claim.CaseLocations.FirstOrDefault().ClaimReport is not null && claim.CaseLocations.FirstOrDefault().ClaimReport?.LocationLongLat is not null)
                {
                    var longLat = claim.CaseLocations.FirstOrDefault().ClaimReport.LocationLongLat.IndexOf("/");
                    var latitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                    var longitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim();

                    var frick = new { Lat = decimal.Parse(latitude), Lng = decimal.Parse(longitude) };
                    return Ok(new { center, dakota, frick });
                }
            }
            else
            {
                var center = new { Lat = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Latitude), Lng = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Longitude) };
                var dakota = new { Lat = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Latitude), Lng = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Longitude) };

                if (claim.CaseLocations.FirstOrDefault().ClaimReport is not null && claim.CaseLocations.FirstOrDefault().ClaimReport?.LocationLongLat is not null)
                {
                    var longLat = claim.CaseLocations.FirstOrDefault().ClaimReport.LocationLongLat.IndexOf("/");
                    var latitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                    var longitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim();

                    var frick = new { Lat = decimal.Parse(latitude), Lng = decimal.Parse(longitude) };
                    return Ok(new { center, dakota, frick });
                }
            }
            return Ok();
        }

        [HttpGet("GetOcrDetail")]
        public IActionResult GetOcrDetail(string claimid)
        {
            var claim = _context.ClaimsInvestigation
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.ClientCompany)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CaseEnabler)
               .Include(c => c.PolicyDetail)
               .ThenInclude(c => c.CostCentre)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.InvestigationCaseSubStatus)
               .Include(c => c.CaseLocations)
               .ThenInclude(c => c.PinCode)
               .Include(c => c.CaseLocations)
                .ThenInclude(c => c.District)
                .Include(c => c.CaseLocations)
                .ThenInclude(c => c.State)
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
               .Include(c => c.CaseLocations)
               .ThenInclude(l => l.ClaimReport)
                .FirstOrDefault(c => c.ClaimsInvestigationId == claimid);

            if (claim.PolicyDetail.ClaimType == ClaimType.HEALTH)
            {
                var center = new { Lat = decimal.Parse(claim.CustomerDetail.PinCode.Latitude), Lng = decimal.Parse(claim.CustomerDetail.PinCode.Longitude) };
                var dakota = new { Lat = decimal.Parse(claim.CustomerDetail.PinCode.Latitude), Lng = decimal.Parse(claim.CustomerDetail.PinCode.Longitude) };

                if (claim.CaseLocations.FirstOrDefault().ClaimReport is not null && claim.CaseLocations.FirstOrDefault().ClaimReport?.OcrLongLat is not null)
                {
                    var longLat = claim.CaseLocations.FirstOrDefault().ClaimReport.OcrLongLat.IndexOf("/");
                    var latitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.OcrLongLat.Substring(0, longLat)?.Trim();
                    var longitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.OcrLongLat.Substring(longLat + 1)?.Trim();

                    var frick = new { Lat = decimal.Parse(latitude), Lng = decimal.Parse(longitude) };
                    return Ok(new { center, dakota, frick });
                }
            }
            else
            {
                var center = new { Lat = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Latitude), Lng = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Longitude) };
                var dakota = new { Lat = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Latitude), Lng = decimal.Parse(claim.CaseLocations.FirstOrDefault().PinCode.Longitude) };

                if (claim.CaseLocations.FirstOrDefault().ClaimReport is not null && claim.CaseLocations.FirstOrDefault().ClaimReport?.OcrLongLat is not null)
                {
                    var longLat = claim.CaseLocations.FirstOrDefault().ClaimReport.OcrLongLat.IndexOf("/");
                    var latitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.OcrLongLat.Substring(0, longLat)?.Trim();
                    var longitude = claim.CaseLocations.FirstOrDefault()?.ClaimReport.OcrLongLat.Substring(longLat + 1)?.Trim();

                    var frick = new { Lat = decimal.Parse(latitude), Lng = decimal.Parse(longitude) };
                    return Ok(new { center, dakota, frick });
                }
            }
            return Ok();
        }
    }
}