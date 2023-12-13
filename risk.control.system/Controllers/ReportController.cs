using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using SmartBreadcrumbs.Attributes;
using risk.control.system.Models.ViewModel;
using risk.control.system.Models;
using System.Text.RegularExpressions;
using risk.control.system.Helpers;
using risk.control.system.Services;
using risk.control.system.AppConstant;

namespace risk.control.system.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpClientService httpClientService;
        private readonly HttpClient _httpClient;
        private Regex longLatRegex = new Regex("(?<lat>[-|+| ]\\d+.\\d+)\\s* \\/\\s*(?<lon>\\d+.\\d+)");

        public ReportController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IHttpClientService httpClientService)
        {
            this._context = context;
            this.webHostEnvironment = webHostEnvironment;
            this.httpClientService = httpClientService;
            _httpClient = new HttpClient();
        }

        [Breadcrumb(title: " Report", FromController = typeof(ClaimsInvestigationController))]
        public IActionResult Index()
        {
            return View();
        }

        [Breadcrumb(" Detail")]
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
              .Include(c => c.PolicyDetail)
              .ThenInclude(c => c.CostCentre)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.InvestigationCaseSubStatus)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.PinCode)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.BeneficiaryRelation)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.District)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.State)
              .Include(c => c.CaseLocations)
              .ThenInclude(c => c.Country)
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

            var location = await _context.CaseLocation
                .Include(l => l.ClaimReport)
                .Include(l => l.Vendor)
                .FirstOrDefaultAsync(l => l.ClaimsInvestigationId == id);

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

            if (location.ClaimReport.LocationLongLat != null)
            {
                var longLat = location.ClaimReport.LocationLongLat.IndexOf("/");
                var latitude = location.ClaimReport.LocationLongLat.Substring(0, longLat)?.Trim();
                var longitude = location.ClaimReport.LocationLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();

                var latLongString = latitude + "," + longitude;
                var url = $"https://maps.googleapis.com/maps/api/staticmap?center={latLongString}&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{latLongString}&key={Applicationsettings.GMAPData}";
                ViewBag.LocationUrl = url;
                RootObject rootObject = await httpClientService.GetAddress(latitude, longitude);

                double registeredLatitude = 0;
                double registeredLongitude = 0;
                if (claimsInvestigation.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredLatitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Longitude);
                }
                else
                {
                    registeredLatitude = Convert.ToDouble(location.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(location.PinCode.Longitude);
                }
                var distance = DistanceFinder.GetDistance(registeredLatitude, 222, Convert.ToDouble(latitude), Convert.ToDouble(longitude));

                var address = rootObject.display_name;

                ViewBag.LocationAddress = string.IsNullOrWhiteSpace(rootObject.display_name) ? "12 Heathcote Drive Forest Hill VIC 3131" : address;
            }
            else
            {
                RootObject rootObject = await httpClientService.GetAddress("-37.839542", "145.164834");
                ViewBag.LocationAddress = rootObject.display_name ?? "12 Heathcote Drive Forest Hill VIC 3131";
                ViewBag.LocationUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            }
            if (location.ClaimReport.OcrLongLat != null)
            {
                var longLat = location.ClaimReport.OcrLongLat.IndexOf("/");
                var latitude = location.ClaimReport.OcrLongLat.Substring(0, longLat)?.Trim();
                var longitude = location.ClaimReport.OcrLongLat.Substring(longLat + 1)?.Trim().Replace("/", "").Trim();
                var latLongString = latitude + "," + longitude;
                var url = $"https://maps.googleapis.com/maps/api/staticmap?center={latLongString}&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{latLongString}&key={Applicationsettings.GMAPData}";
                ViewBag.OcrLocationUrl = url;
                RootObject rootObject = await httpClientService.GetAddress(latitude, longitude);

                double registeredLatitude = 0;
                double registeredLongitude = 0;
                if (claimsInvestigation.PolicyDetail.ClaimType == ClaimType.HEALTH)
                {
                    registeredLatitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Latitude);
                    registeredLongitude = Convert.ToDouble(claimsInvestigation.CustomerDetail.PinCode.Longitude);
                }
                var distance = DistanceFinder.GetDistance(registeredLatitude, 222, Convert.ToDouble(latitude), Convert.ToDouble(longitude));

                var address = rootObject.display_name;

                ViewBag.OcrLocationAddress = string.IsNullOrWhiteSpace(rootObject.display_name) ? "12 Heathcote Drive Forest Hill VIC 3131" : address;
            }
            else
            {
                RootObject rootObject = await httpClientService.GetAddress("-37.839542", "145.164834");
                ViewBag.OcrLocationAddress = rootObject.display_name ?? "12 Heathcote Drive Forest Hill VIC 3131";
                ViewBag.OcrLocationUrl = $"https://maps.googleapis.com/maps/api/staticmap?center=32.661839,-97.263680&zoom=14&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C32.661839,-97.263680&key={Applicationsettings.GMAPData}";
            }

            var customerLatLong = claimsInvestigation.CustomerDetail.PinCode.Latitude + "," + claimsInvestigation.CustomerDetail.PinCode.Longitude;

            var curl = $"https://maps.googleapis.com/maps/api/staticmap?center={customerLatLong}&zoom=8&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{customerLatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.CustomerLocationUrl = curl;

            var beneficiarylatLong = location.PinCode.Latitude + "," + location.PinCode.Longitude;
            var bUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={beneficiarylatLong}&zoom=8&size=200x200&maptype=roadmap&markers=color:red%7Clabel:S%7C{beneficiarylatLong}&key={Applicationsettings.GMAPData}";
            ViewBag.BeneficiaryLocationUrl = bUrl;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PrintReport(string id)
        {
            var claim = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .Include(c => c.CustomerDetail)
                .Include(c => c.CaseLocations)
                .ThenInclude(r => r.ClaimReport)
                .FirstOrDefault(c => c.ClaimsInvestigationId == id);

            var policy = claim.PolicyDetail;
            var customer = claim.CustomerDetail;
            var beneficiary = claim.CaseLocations.FirstOrDefault();
            var report = claim.CaseLocations.FirstOrDefault()?.ClaimReport;

            string folder = Path.Combine(webHostEnvironment.WebRootPath, "report");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filename = "report" + id + ".pdf";

            var filePath = Path.Combine(webHostEnvironment.WebRootPath, "report", filename);

            ReportRunner.Run(webHostEnvironment.WebRootPath).Build(filePath); ;
            var memory = new MemoryStream();
            using var stream = new FileStream(filePath, FileMode.Open);
            await stream.CopyToAsync(memory);
            memory.Position = 0;
            return File(memory, "application/pdf", filename);
        }

        [HttpGet]
        public async Task<IActionResult> PrintPdfReport(string id)
        {
            var claim = _context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .Include(c => c.CustomerDetail)
                .Include(c => c.CaseLocations)
                .ThenInclude(r => r.ClaimReport)
                .FirstOrDefault(c => c.ClaimsInvestigationId == id);

            var policy = claim.PolicyDetail;
            var customer = claim.CustomerDetail;
            var beneficiary = claim.CaseLocations.FirstOrDefault();
            var report = claim.CaseLocations.FirstOrDefault()?.ClaimReport;

            string folder = Path.Combine(webHostEnvironment.WebRootPath, "report");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filename = "report" + id + ".pdf";

            var filePath = Path.Combine(webHostEnvironment.WebRootPath, "report", filename);

            PdfReportRunner.Run(webHostEnvironment.WebRootPath).Build(filePath); ;
            var memory = new MemoryStream();
            using var stream = new FileStream(filePath, FileMode.Open);
            await stream.CopyToAsync(memory);
            memory.Position = 0;
            return File(memory, "application/pdf", filename);
        }

        public async Task<RootObject> GetAddress(string lat, string lon)
        {
            var jsonData = await _httpClient.GetStringAsync("http://nominatim.openstreetmap.org/reverse?format=json&lat=" + lat + "&lon=" + lon);

            var rootObject = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(jsonData);
            return rootObject;
        }
    }
}