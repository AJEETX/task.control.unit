using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Claims;

using static System.Net.WebRequestMethods;

namespace risk.control.system.Controllers
{
    public class ClaimsVendorController : Controller
    {
        public List<UsersViewModel> UserList;
        private readonly IClaimsInvestigationService claimsInvestigationService;
        private readonly UserManager<VendorApplicationUser> userManager;
        private readonly IDashboardService dashboardService;
        private readonly IMailboxService mailboxService;
        private readonly IToastNotification toastNotification;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private static HttpClient httpClient = new();

        public ClaimsVendorController(
            IClaimsInvestigationService claimsInvestigationService,
            UserManager<VendorApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IDashboardService dashboardService,
            IMailboxService mailboxService,
            IToastNotification toastNotification,
            ApplicationDbContext context)
        {
            this.claimsInvestigationService = claimsInvestigationService;
            this.userManager = userManager;
            this.dashboardService = dashboardService;
            this.mailboxService = mailboxService;
            this.toastNotification = toastNotification;
            this._context = context;
            this.webHostEnvironment = webHostEnvironment;
            UserList = new List<UsersViewModel>();
        }

        [Breadcrumb(" Allocate To Agent")]
        public async Task<IActionResult> AllocateToVendorAgent(string selectedcase)
        {
            if (string.IsNullOrWhiteSpace(selectedcase))
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be allocate.");
                return RedirectToAction(nameof(Index));
            }

            if (_context.ClaimsInvestigation == null)
            {
                return NotFound();
            }
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var allocatedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR);

            var claimsInvestigation = _context.ClaimsInvestigation
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
                .FirstOrDefault(m => m.ClaimsInvestigationId == selectedcase && m.CaseLocations.Any(c => c.VendorId == vendorUser.VendorId));
            claimsInvestigation.CaseLocations = claimsInvestigation.CaseLocations.Where(c => c.VendorId == vendorUser.VendorId
                        && c.InvestigationCaseSubStatusId == allocatedStatus.InvestigationCaseSubStatusId)?.ToList();

            if (claimsInvestigation == null)
            {
                return NotFound();
            }
            return View(claimsInvestigation);
        }

        [HttpGet]
        [Breadcrumb("Allocate")]
        public async Task<IActionResult> SelectVendorAgent(string selectedcase)
        {
            if (string.IsNullOrWhiteSpace(selectedcase))
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be allocate.");
                return RedirectToAction(nameof(Index));
            }

            var userEmail = HttpContext.User?.Identity?.Name;
            var allocatedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ALLOCATED_TO_VENDOR);

            var claimsCaseToAllocateToVendorAgent = _context.ClaimsInvestigation
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
                .Include(c => c.Vendors)
                .FirstOrDefault(v => v.ClaimsInvestigationId == selectedcase);

            var claimsCaseLocation = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.InvestigationCaseSubStatus)
                .Include(c => c.Vendor)
                .Include(c => c.PinCode)
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.Country)
                .FirstOrDefault(c => c.CaseLocationId == claimsCaseToAllocateToVendorAgent.CaseLocations.FirstOrDefault().CaseLocationId &&
                c.InvestigationCaseSubStatusId == allocatedStatus.InvestigationCaseSubStatusId);

            var agentRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Agent.ToString()));

            var vendorUsers = _context.VendorApplicationUser
                .Include(u => u.District)
                .Include(u => u.State)
                .Include(u => u.Country)
                .Include(u => u.PinCode)
                .Where(u => u.VendorId == claimsCaseLocation.VendorId && u.Active);

            List<VendorUserClaim> agents = new List<VendorUserClaim>();
            var result = dashboardService.CalculateAgentCaseStatus(userEmail);

            foreach (var vendorUser in vendorUsers)
            {
                var isTrue = await userManager.IsInRoleAsync(vendorUser, agentRole?.Name);
                if (isTrue)
                {
                    int claimCount = 0;
                    if (result.TryGetValue(vendorUser.Email, out claimCount))
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = vendorUser,
                            CurrentCaseCount = claimCount,
                        };
                        agents.Add(agentData);
                    }
                    else
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = vendorUser,
                            CurrentCaseCount = 0,
                        };
                        agents.Add(agentData);
                    }
                }
            }

            var model = new ClaimsInvestigationVendorAgentModel
            {
                CaseLocation = claimsCaseLocation,
                ClaimsInvestigation = claimsCaseToAllocateToVendorAgent,
                VendorUserClaims = agents
            };

            var customerLatLong = claimsCaseToAllocateToVendorAgent.CustomerDetail.PinCode.Latitude + "," + claimsCaseToAllocateToVendorAgent.CustomerDetail.PinCode.Longitude;

            var url = $"https://maps.googleapis.com/maps/api/staticmap?center={customerLatLong}&zoom=8&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{customerLatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.CustomerLocationUrl = url;

            var beneficiarylatLong = claimsCaseLocation.PinCode.Latitude + "," + claimsCaseLocation.PinCode.Longitude;
            var bUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={beneficiarylatLong}&zoom=8&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{beneficiarylatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.BeneficiaryLocationUrl = bUrl;

            return View(model);
        }

        [HttpGet]
        [Breadcrumb("ReAllocate")]
        public async Task<IActionResult> ReSelectVendorAgent(string selectedcase)
        {
            if (string.IsNullOrWhiteSpace(selectedcase))
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be allocate.");
                return RedirectToAction(nameof(Index));
            }

            var userEmail = HttpContext.User?.Identity?.Name;
            var submittedStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                        i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR);

            var claimsCaseToAllocateToVendorAgent = _context.ClaimsInvestigation
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
                .Include(c => c.Vendors)
                .FirstOrDefault(v => v.ClaimsInvestigationId == selectedcase);

            var location = claimsCaseToAllocateToVendorAgent.CaseLocations.FirstOrDefault();

            var claimsCaseLocation = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.InvestigationCaseSubStatus)
                .Include(c => c.Vendor)
                .Include(c => c.PinCode)
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.District)
                .Include(c => c.State)
                .Include(c => c.Country)
                .FirstOrDefault(c => c.CaseLocationId == location.CaseLocationId &&
                c.InvestigationCaseSubStatusId == submittedStatus.InvestigationCaseSubStatusId);

            var agentRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Agent.ToString()));

            var vendorUsers = _context.VendorApplicationUser
                .Include(u => u.District)
                .Include(u => u.State)
                .Include(u => u.Country)
                .Include(u => u.PinCode)
                .Where(u => u.VendorId == claimsCaseLocation.VendorId && u.Active);

            List<VendorUserClaim> agents = new List<VendorUserClaim>();
            var result = dashboardService.CalculateAgentCaseStatus(userEmail);

            foreach (var vendorUser in vendorUsers)
            {
                var isTrue = await userManager.IsInRoleAsync(vendorUser, agentRole?.Name);
                if (isTrue)
                {
                    int claimCount = 0;
                    if (result.TryGetValue(vendorUser.Email, out claimCount))
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = vendorUser,
                            CurrentCaseCount = claimCount,
                        };
                        agents.Add(agentData);
                    }
                    else
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = vendorUser,
                            CurrentCaseCount = 0,
                        };
                        agents.Add(agentData);
                    }
                }
            }

            var model = new ClaimsInvestigationVendorAgentModel
            {
                CaseLocation = claimsCaseLocation,
                ClaimsInvestigation = claimsCaseToAllocateToVendorAgent,
                VendorUserClaims = agents
            };

            var customerLatLong = claimsCaseToAllocateToVendorAgent.CustomerDetail.PinCode.Latitude + "," + claimsCaseToAllocateToVendorAgent.CustomerDetail.PinCode.Longitude;

            var url = $"https://maps.googleapis.com/maps/api/staticmap?center={customerLatLong}&zoom=8&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{customerLatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.CustomerLocationUrl = url;

            var beneficiarylatLong = claimsCaseLocation.PinCode.Latitude + "," + claimsCaseLocation.PinCode.Longitude;
            var bUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={beneficiarylatLong}&zoom=8&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{beneficiarylatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.BeneficiaryLocationUrl = bUrl;

            location.ClaimReport = null;

            _context.SaveChanges();

            return View(model);
        }

        [Breadcrumb("Agency Workload")]
        public async Task<IActionResult> AgentLoad()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var agentRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Agent.ToString()));
            List<VendorUserClaim> agents = new List<VendorUserClaim>();

            var vendor = _context.Vendor
                .Include(c => c.VendorApplicationUser)
                .FirstOrDefault(c => c.VendorId == vendorUser.VendorId);

            var users = vendor.VendorApplicationUser.AsQueryable();
            var result = dashboardService.CalculateAgentCaseStatus(userEmail);

            foreach (var user in users)
            {
                var isAgent = await userManager.IsInRoleAsync(user, agentRole?.Name);
                if (isAgent)
                {
                    int claimCount = 0;
                    if (result.TryGetValue(user.Email, out claimCount))
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = user,
                            CurrentCaseCount = claimCount,
                        };
                        agents.Add(agentData);
                    }
                    else
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = user,
                            CurrentCaseCount = 0,
                        };
                        agents.Add(agentData);
                    }
                }
            }
            return View(agents);
        }

        [HttpPost]
        public async Task<IActionResult> AllocateToVendorAgent(string selectedcase, string claimId, long caseLocationId)
        {
            if (string.IsNullOrWhiteSpace(selectedcase) || string.IsNullOrWhiteSpace(claimId) || caseLocationId < 1)
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be allocate.");
                return RedirectToAction(nameof(Index));
            }

            var userEmail = HttpContext.User?.Identity?.Name;

            var vendorAgent = _context.VendorApplicationUser.FirstOrDefault(c => c.Id.ToString() == selectedcase);

            var claim = await claimsInvestigationService.AssignToVendorAgent(vendorAgent.Email, userEmail, vendorAgent.VendorId, claimId);

            await mailboxService.NotifyClaimAssignmentToVendorAgent(userEmail, claimId, vendorAgent.Email, vendorAgent.VendorId, caseLocationId);

            toastNotification.AddSuccessToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> Claim [Policy # {0}] tasked to {1} successfully!", claim.PolicyDetail.ContractNumber, vendorAgent.Email));

            return RedirectToAction(nameof(ClaimsVendorController.Index), "ClaimsVendor");
        }

        [Breadcrumb(" Claims")]
        public ActionResult Index()
        {
            var activePage = new MvcBreadcrumbNode("Open", "ClaimsVendor", "Claims");
            var newPage = new MvcBreadcrumbNode("Index", "ClaimsVendor", "Allocate") { Parent = activePage };
            ViewData["BreadcrumbNode"] = newPage;

            var userRole = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (userRole.Value.Contains(AppRoles.Agent.ToString()))
            {
                return View("Agent");
            }
            return View();
        }

        [Breadcrumb(" Tasks")]
        public ActionResult Agent()
        {
            //var activePage = new MvcBreadcrumbNode("Open", "ClaimsVendor", "Claims");
            //var newPage = new MvcBreadcrumbNode("Index", "ClaimsVendor", "Allocate New") { Parent = activePage };
            //ViewData["BreadcrumbNode"] = newPage;
            return View();
        }

        [Breadcrumb("Agent Report", FromAction = "Agent")]
        public async Task<IActionResult> GetInvestigate(string selectedcase)
        {
            if (string.IsNullOrWhiteSpace(selectedcase))
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be investigate.");
                return RedirectToAction(nameof(Index));
            }

            var currentUserEmail = HttpContext.User?.Identity?.Name;

            var claimsInvestigation = _context.ClaimsInvestigation
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
                .FirstOrDefault(c => c.ClaimsInvestigationId == selectedcase);
            var assignedToAgentStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                       i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT);
            var claimCase = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.PinCode)
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.ClaimReport)
                .Include(c => c.District)
                .Include(c => c.Country)
                .Include(c => c.State)
                .FirstOrDefault(c => c.ClaimsInvestigationId == selectedcase
                && c.InvestigationCaseSubStatusId == assignedToAgentStatus.InvestigationCaseSubStatusId
                    );

            if (claimCase.ClaimReport.LocationLongLat != null)
            {
                var longLat = claimCase.ClaimReport.LocationLongLat.IndexOf("/");
                var latitude = claimCase.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                var longitude = claimCase.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();
                var latLongString = latitude + "," + longitude;
                var url = $"https://maps.googleapis.com/maps/api/staticmap?center={latLongString}&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{latLongString}&key={Applicationsettings.GMAPData}";
                ViewBag.LocationUrl = url;

                RootObject rootObject = getAddress((latitude), (longitude));

                double registeredLatitude = 0;
                double registeredLongitude = 0;
                if (claimsInvestigation.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredLatitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Longitude);
                }
                else
                {
                    registeredLatitude = Convert.ToDouble(claimCase.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimCase.PinCode.Longitude);
                }
                var distance = DistanceFinder.GetDistance(registeredLatitude, registeredLongitude, Convert.ToDouble(latitude), Convert.ToDouble(longitude));

                var address = rootObject.display_name;

                ViewBag.LocationAddress = string.IsNullOrWhiteSpace(rootObject.display_name) ? "12 Heathcote Drive Forest Hill VIC 3131" : address;
            }
            else
            {
                var latitude = "-37.839542";
                var longitude = "145.164834";
                var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,windspeed_10m&hourly=temperature_2m,relativehumidity_2m,windspeed_10m";

                RootObject rootObject = getAddress(latitude, longitude);
                ViewBag.LocationAddress = rootObject.display_name ?? "12 Heathcote Drive Forest Hill VIC 3131";

                var weatherData = await httpClient.GetFromJsonAsync<Weather>(weatherUrl);
                string weatherCustomData = $"Temperature:{weatherData.current.temperature_2m} {weatherData.current_units.temperature_2m}.\nWindspeed:{weatherData.current.windspeed_10m} {weatherData.current_units.windspeed_10m} \nElevation(sea level):{weatherData.elevation} metres";
                claimCase.ClaimReport.LocationData = weatherCustomData;

                ViewBag.LocationUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            }

            if (claimCase.ClaimReport.OcrLongLat != null)
            {
                var longLat = claimCase.ClaimReport.OcrLongLat.IndexOf("/");
                var latitude = claimCase.ClaimReport.OcrLongLat.Substring(0, longLat)?.Trim();
                var longitude = claimCase.ClaimReport.OcrLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();
                var latLongString = latitude + "," + longitude;
                var url = $"https://maps.googleapis.com/maps/api/staticmap?center={latLongString}&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{latLongString}&key={Applicationsettings.GMAPData}";
                ViewBag.OcrLocationUrl = url;

                RootObject rootObject = getAddress((latitude), (longitude));
                double registeredLatitude = 0;
                double registeredLongitude = 0;
                if (claimsInvestigation.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredLatitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Longitude);
                }

                var distance = DistanceFinder.GetDistance(registeredLatitude, registeredLongitude, Convert.ToDouble(latitude), Convert.ToDouble(longitude));

                var address = rootObject.display_name;

                ViewBag.OcrLocationAddress = string.IsNullOrWhiteSpace(rootObject.display_name) ? "12 Heathcote Drive Forest Hill VIC 3131" : address;
            }
            else
            {
                var latitude = "-37.839542";
                var longitude = "145.164834";

                RootObject rootObject = getAddress(latitude, longitude);
                ViewBag.OcrLocationAddress = rootObject.display_name ?? "12 Heathcote Drive Forest Hill VIC 3131";
                ViewBag.OcrLocationUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            }

            var customerLatLong = claimsInvestigation.CustomerDetail.PinCode.Latitude + "," + claimsInvestigation.CustomerDetail.PinCode.Longitude;

            var curl = $"https://maps.googleapis.com/maps/api/staticmap?center={customerLatLong}&zoom=8&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{customerLatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.CustomerLocationUrl = curl;

            var beneficiarylatLong = claimCase.PinCode.Latitude + "," + claimCase.PinCode.Longitude;
            var bUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={beneficiarylatLong}&zoom=8&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{beneficiarylatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.BeneficiaryLocationUrl = bUrl;

            return View(new ClaimsInvestigationVendorsModel { CaseLocation = claimCase, ClaimsInvestigation = claimsInvestigation });
        }

        public static RootObject getAddress(string lat, string lon)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            webClient.Headers.Add("Referer", "http://www.i-taskify.co.in");
            var jsonData = webClient.DownloadData("http://nominatim.openstreetmap.org/reverse?format=json&lat=" + lat + "&lon=" + lon);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(RootObject));
            RootObject rootObject = (RootObject)ser.ReadObject(new MemoryStream(jsonData));
            return rootObject;
        }

        [Breadcrumb(" Review Report")]
        public async Task<IActionResult> GetInvestigateReportReview(string selectedcase)
        {
            if (string.IsNullOrWhiteSpace(selectedcase))
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be review.");
                return RedirectToAction(nameof(Index));
            }

            var currentUserEmail = HttpContext.User?.Identity?.Name;

            var claimsInvestigation = _context.ClaimsInvestigation
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
                .FirstOrDefault(c => c.ClaimsInvestigationId == selectedcase);
            var submittedToSupervisortStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                       i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.SUBMITTED_TO_SUPERVISOR);
            var claimCase = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.PinCode)
                .Include(c => c.ClaimReport)
                .Include(c => c.District)
                .Include(c => c.State)
                .FirstOrDefault(c => (c.ClaimsInvestigationId == selectedcase
                && c.InvestigationCaseSubStatusId == submittedToSupervisortStatus.InvestigationCaseSubStatusId) || c.IsReviewCaseLocation
                    );
            var agentRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.Agent.ToString()));

            var vendorUsers = _context.VendorApplicationUser.Where(u => u.VendorId == claimCase.VendorId);

            List<VendorUserClaim> agents = new List<VendorUserClaim>();
            var result = dashboardService.CalculateAgentCaseStatus(currentUserEmail);

            foreach (var vendorUser in vendorUsers)
            {
                var isTrue = await userManager.IsInRoleAsync(vendorUser, agentRole?.Name);
                if (isTrue)
                {
                    int claimCount = 0;
                    if (result.TryGetValue(vendorUser.Email, out claimCount))
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = vendorUser,
                            CurrentCaseCount = claimCount,
                        };
                        agents.Add(agentData);
                    }
                    else
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = vendorUser,
                            CurrentCaseCount = 0,
                        };
                        agents.Add(agentData);
                    }
                }
            }
            return View(new ClaimsInvestigationVendorAgentModel
            {
                CaseLocation = claimCase,
                ClaimsInvestigation = claimsInvestigation,
                VendorUserClaims = agents
            }
            );
        }

        [Breadcrumb("Agent Report")]
        public async Task<IActionResult> GetInvestigateReport(string selectedcase)
        {
            if (string.IsNullOrWhiteSpace(selectedcase))
            {
                toastNotification.AddAlertToastMessage("No case selected!!!. Please select case to be investigate.");
                return RedirectToAction(nameof(Index));
            }

            var currentUserEmail = HttpContext.User?.Identity?.Name;

            var claimsInvestigation = _context.ClaimsInvestigation
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
              .ThenInclude(c => c.BeneficiaryRelation)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.ClaimReport)
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
                .FirstOrDefault(c => c.ClaimsInvestigationId == selectedcase);

            var claimCase = _context.CaseLocation
                .Include(c => c.ClaimsInvestigation)
                .Include(c => c.PinCode)
                .Include(c => c.BeneficiaryRelation)
                .Include(c => c.ClaimReport)
                .Include(c => c.District)
                .Include(c => c.Country)
                .Include(c => c.State)
                .FirstOrDefault(c => c.ClaimsInvestigationId == selectedcase);
            var assignedToAgentStatus = _context.InvestigationCaseSubStatus.FirstOrDefault(
                       i => i.Name.ToUpper() == CONSTANTS.CASE_STATUS.CASE_SUBSTATUS.ASSIGNED_TO_AGENT);

            if (claimCase.ClaimReport.LocationLongLat != null)
            {
                var longLat = claimCase.ClaimReport.LocationLongLat.IndexOf("/");
                var latitude = claimCase.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                var longitude = claimCase.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();
                var latLongString = latitude + "," + longitude;
                var url = $"https://maps.googleapis.com/maps/api/staticmap?center={latLongString}&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{latLongString}&key={Applicationsettings.GMAPData}";
                ViewBag.LocationUrl = url;
                RootObject rootObject = getAddress((latitude), (longitude));
                double registeredLatitude = 0;
                double registeredLongitude = 0;
                if (claimsInvestigation.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredLatitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Longitude);
                }
                else
                {
                    registeredLatitude = Convert.ToDouble(claimCase.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimCase.PinCode.Longitude);
                }
                var distance = DistanceFinder.GetDistance(registeredLatitude, registeredLongitude, Convert.ToDouble(latitude), Convert.ToDouble(longitude));

                var address = rootObject.display_name;

                ViewBag.LocationAddress = string.IsNullOrWhiteSpace(rootObject.display_name) ? "12 Heathcote Drive Forest Hill VIC 3131" : address;
            }
            else
            {
                RootObject rootObject = getAddress("-37.839542", "145.164834");
                ViewBag.LocationAddress = rootObject.display_name ?? "12 Heathcote Drive Forest Hill VIC 3131";
                ViewBag.LocationUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            }

            if (claimCase.ClaimReport.OcrLongLat != null)
            {
                var longLat = claimCase.ClaimReport.OcrLongLat.IndexOf("/");
                var latitude = claimCase.ClaimReport.OcrLongLat.Substring(0, longLat)?.Trim();
                var longitude = claimCase.ClaimReport.OcrLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();
                var latLongString = latitude + "," + longitude;
                var url = $"https://maps.googleapis.com/maps/api/staticmap?center={latLongString}&zoom=14&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{latLongString}&key={Applicationsettings.GMAPData}";
                ViewBag.OcrLocationUrl = url;
                RootObject rootObject = getAddress((latitude), (longitude));
                double registeredLatitude = 0;
                double registeredLongitude = 0;
                if (claimsInvestigation.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredLatitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Longitude);
                }

                var distance = DistanceFinder.GetDistance(registeredLatitude, registeredLongitude, Convert.ToDouble(latitude), Convert.ToDouble(longitude));

                var address = rootObject.display_name;

                ViewBag.OcrLocationAddress = string.IsNullOrWhiteSpace(rootObject.display_name) ? "12 Heathcote Drive Forest Hill VIC 3131" : address;
            }
            else
            {
                RootObject rootObject = getAddress("-37.839542", "145.164834");
                ViewBag.OcrLocationAddress = rootObject.display_name ?? "12 Heathcote Drive Forest Hill VIC 3131";
                ViewBag.OcrLocationUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            }

            var customerLatLong = claimsInvestigation.CustomerDetail.PinCode.Latitude + "," + claimsInvestigation.CustomerDetail.PinCode.Longitude;

            var curl = $"https://maps.googleapis.com/maps/api/staticmap?center={customerLatLong}&zoom=8&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{customerLatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.CustomerLocationUrl = curl;

            var beneficiarylatLong = claimCase.PinCode.Latitude + "," + claimCase.PinCode.Longitude;
            var bUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={beneficiarylatLong}&zoom=8&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{beneficiarylatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.BeneficiaryLocationUrl = bUrl;

            return View(new ClaimsInvestigationVendorsModel { CaseLocation = claimCase, ClaimsInvestigation = claimsInvestigation });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReport(string remarks, string question1, string question2, string question3, string question4, string claimId, long caseLocationId)
        {
            if (string.IsNullOrWhiteSpace(remarks) || string.IsNullOrWhiteSpace(claimId) || caseLocationId < 1)
            {
                toastNotification.AddAlertToastMessage("No Agent remarks entered!!!. Please enter remarks.");
                return RedirectToAction(nameof(GetInvestigate), new { selectedcase = claimId });
            }

            string userEmail = HttpContext?.User?.Identity.Name;

            var claim = await claimsInvestigationService.SubmitToVendorSupervisor(userEmail, caseLocationId, claimId, remarks, question1, question2, question3, question4);

            await mailboxService.NotifyClaimReportSubmitToVendorSupervisor(userEmail, claimId, caseLocationId);

            toastNotification.AddSuccessToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> Claim [Policy # {0}] investigation submitted to supervisor successfully !", claim.PolicyDetail.ContractNumber));

            return RedirectToAction(nameof(ClaimsVendorController.Agent), "ClaimsVendor");
        }

        [HttpPost]
        public async Task<IActionResult> ProcessReport(string supervisorRemarks, string supervisorRemarkType, string claimId, long caseLocationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(supervisorRemarks) || string.IsNullOrWhiteSpace(claimId) || caseLocationId < 1)
                {
                    toastNotification.AddAlertToastMessage("No Supervisor remarks entered!!!. Please enter remarks.");
                    return RedirectToAction(nameof(GetInvestigateReport), new { selectedcase = claimId });
                }
                string userEmail = HttpContext?.User?.Identity.Name;
                var reportUpdateStatus = SupervisorRemarkType.OK;

                var success = await claimsInvestigationService.ProcessAgentReport(userEmail, supervisorRemarks, caseLocationId, claimId, reportUpdateStatus);

                if (success != null)
                {
                    await mailboxService.NotifyClaimReportSubmitToCompany(userEmail, claimId, caseLocationId);
                    toastNotification.AddSuccessToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> Claim [Policy # {0}] report submitted to Company successfully !", success.PolicyDetail.ContractNumber));
                }
                else
                {
                    toastNotification.AddSuccessToastMessage("Report sent to review successfully");
                }
                return RedirectToAction(nameof(ClaimsVendorController.ClaimReport));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReAllocateReport(string supervisorRemarks, string supervisorRemarkType, string claimId, long caseLocationId)
        {
            if (string.IsNullOrWhiteSpace(supervisorRemarks) || string.IsNullOrWhiteSpace(claimId) || caseLocationId < 1)
            {
                toastNotification.AddAlertToastMessage("No remarks entered!!!. Please enter remarks.");
                return RedirectToAction(nameof(GetInvestigate), new { selectedcase = claimId });
            }
            string userEmail = HttpContext?.User?.Identity.Name;
            var reportUpdateStatus = SupervisorRemarkType.REVIEW;

            var success = await claimsInvestigationService.ProcessAgentReport(userEmail, supervisorRemarks, caseLocationId, claimId, reportUpdateStatus);

            if (success != null)
            {
                await mailboxService.NotifyClaimReportSubmitToCompany(userEmail, claimId, caseLocationId);
                toastNotification.AddSuccessToastMessage("report submitted to Company successfully");
            }
            else
            {
                toastNotification.AddSuccessToastMessage("Report sent to review successfully");
            }
            return RedirectToAction(nameof(ClaimsVendorController.Index), "ClaimsVendor");
        }

        [Breadcrumb(" Active")]
        public IActionResult Open()
        {
            return View();
        }

        [Breadcrumb(title: " Detail", FromAction = "Open")]
        public async Task<IActionResult> Detail(string id)
        {
            if (id == null || _context.ClaimsInvestigation == null)
            {
                return NotFound();
            }

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

            if (claimsInvestigation == null)
            {
                return NotFound();
            }
            var model = new ClaimTransactionModel
            {
                Claim = claimsInvestigation,
                Log = caseLogs,
                Location = location
            };

            var customerLatLong = claimsInvestigation.CustomerDetail.PinCode.Latitude + "," + claimsInvestigation.CustomerDetail.PinCode.Longitude;

            var curl = $"https://maps.googleapis.com/maps/api/staticmap?center={customerLatLong}&zoom=8&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{customerLatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.CustomerLocationUrl = curl;

            var beneficiarylatLong = location.PinCode.Latitude + "," + location.PinCode.Longitude;
            var bUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={beneficiarylatLong}&zoom=8&size=100x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{beneficiarylatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.BeneficiaryLocationUrl = bUrl;

            return View(model);
        }

        [Breadcrumb("Agent Report")]
        public async Task<IActionResult> ClaimReport()
        {
            return View();
        }

        [Breadcrumb(" Re Allocate")]
        public async Task<IActionResult> ClaimReportReview()
        {
            return View();
        }
    }
}