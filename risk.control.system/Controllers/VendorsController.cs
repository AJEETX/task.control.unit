using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

namespace risk.control.system.Controllers
{
    public class VendorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;
        private readonly IWebHostEnvironment webHostEnvironment;

        public VendorsController(ApplicationDbContext context, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.toastNotification = toastNotification;
            this.webHostEnvironment = webHostEnvironment;
        }

        // GET: Vendors
        [Breadcrumb("All Agencies")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: Vendors/Details/5
        [Breadcrumb(" Manage Agency")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return NotFound();
            }

            var vendor = await _context.Vendor
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.District)
                .Include(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.PincodeServices)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.District)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.LineOfBusiness)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.InvestigationServiceType)
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }
            //var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "Agencies");
            //var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            //var editPage = new MvcBreadcrumbNode("Edit", "Vendors", $"Edit") { Parent = agencyPage, RouteValues = new { id = id } };
            //ViewData["BreadcrumbNode"] = editPage;

            return View(vendor);
        }

        [Breadcrumb("Manage Service")]
        public async Task<IActionResult> Service(string id)
        {
            if (id == null || _context.Vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return NotFound();
            }
            ViewData["vendorId"] = id;
            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Service", "Vendors", $"Services") { Parent = agencyPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = editPage;

            return View();
        }

        // GET: Vendors/Create
        [Breadcrumb(" Add Agency")]
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            return View();
        }

        // POST: Vendors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vendor vendor, string domainAddress, string mailAddress)
        {
            try
            {
                if (vendor is not null)
                {
                    Domain domainData = (Domain)Enum.Parse(typeof(Domain), domainAddress, true);

                    vendor.Email = mailAddress.ToLower() + domainData.GetEnumDisplayName();

                    IFormFile? vendorDocument = Request.Form?.Files?.FirstOrDefault();
                    if (vendorDocument is not null)
                    {
                        string newFileName = vendor.Email + Guid.NewGuid().ToString();
                        string fileExtension = Path.GetExtension(vendorDocument.FileName);
                        newFileName += fileExtension;
                        var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);
                        vendorDocument.CopyTo(new FileStream(upload, FileMode.Create));
                        vendor.DocumentUrl = "/img/" + newFileName;

                        using var dataStream = new MemoryStream();
                        vendorDocument.CopyTo(dataStream);
                        vendor.DocumentImage = dataStream.ToArray();
                    }
                    vendor.Status = VendorStatus.ACTIVE;
                    vendor.ActivatedDate = DateTime.UtcNow;
                    vendor.DomainName = domainData;
                    vendor.Updated = DateTime.UtcNow;
                    vendor.UpdatedBy = HttpContext.User?.Identity?.Name;

                    _context.Add(vendor);
                    await _context.SaveChangesAsync();

                    var response = SmsService.SendSingleMessage(vendor.PhoneNumber, "Agency created. Domain : " + vendor.Email);

                    toastNotification.AddSuccessToastMessage("agency created successfully!");
                    return RedirectToAction(nameof(Index));
                }
                toastNotification.AddErrorToastMessage("Error to create agency!");
                return Problem();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: Vendors/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Vendor == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendor.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            var country = _context.Country.Where(c => c.CountryId == vendor.CountryId);
            var relatedStates = _context.State.Include(s => s.Country).Where(s => s.Country.CountryId == vendor.CountryId).OrderBy(d => d.Name);
            var districts = _context.District.Include(d => d.State).Where(d => d.State.StateId == vendor.StateId).OrderBy(d => d.Name);
            var pincodes = _context.PinCode.Include(d => d.District).Where(d => d.District.DistrictId == vendor.DistrictId).OrderBy(d => d.Name);

            ViewData["CountryId"] = new SelectList(country.OrderBy(c => c.Name), "CountryId", "Name", vendor.CountryId);
            ViewData["StateId"] = new SelectList(relatedStates, "StateId", "Name", vendor.StateId);
            ViewData["DistrictId"] = new SelectList(districts, "DistrictId", "Name", vendor.DistrictId);
            ViewData["PinCodeId"] = new SelectList(pincodes, "PinCodeId", "Code", vendor.PinCodeId);

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Edit", "Vendors", $"Edit Agency") { Parent = agencyPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = editPage;

            return View(vendor);
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string vendorId, Vendor vendor)
        {
            if (vendorId != vendor.VendorId)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return NotFound();
            }

            if (vendor is not null)
            {
                try
                {
                    IFormFile? vendorDocument = Request.Form?.Files?.FirstOrDefault();
                    if (vendorDocument is not null)
                    {
                        string newFileName = vendor.Email + Guid.NewGuid().ToString();
                        string fileExtension = Path.GetExtension(vendorDocument.FileName);
                        newFileName += fileExtension;
                        var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);

                        using var dataStream = new MemoryStream();
                        vendorDocument.CopyTo(dataStream);
                        vendor.DocumentImage = dataStream.ToArray();
                        vendorDocument.CopyTo(new FileStream(upload, FileMode.Create));
                        vendor.DocumentUrl = "/img/" + newFileName;
                    }
                    else
                    {
                        var existingVendor = await _context.Vendor.AsNoTracking().FirstOrDefaultAsync(c => c.VendorId == vendorId);
                        if (existingVendor.DocumentImage != null)
                        {
                            vendor.DocumentImage = existingVendor.DocumentImage;
                            vendor.DocumentUrl = existingVendor.DocumentUrl;
                        }
                    }
                    vendor.Updated = DateTime.UtcNow;
                    vendor.UpdatedBy = HttpContext.User?.Identity?.Name;

                    _context.Vendor.Update(vendor);

                    // var userEmail = HttpContext.User?.Identity?.Name;
                    // var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);
                    // var company = _context.ClientCompany
                    //.Include(c => c.EmpanelledVendors)
                    //.AsNoTracking()
                    //.FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);
                    // if (company != null)
                    // {
                    //     var empanelledVendors = company.EmpanelledVendors?.ToList();
                    //     var existingVendor = empanelledVendors.FirstOrDefault(e => e.VendorId == vendor.VendorId);
                    //     if (existingVendor != null)
                    //     {
                    //         company.EmpanelledVendors.Remove(existingVendor);
                    //         company.EmpanelledVendors.Add(vendor);
                    //         _context.ClientCompany.Update(company);
                    //     }
                    // }
                    var response = SmsService.SendSingleMessage(vendor.PhoneNumber, "Agency edited. Domain : " + vendor.Email);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendor.VendorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("agency edited successfully!");
                return RedirectToAction(nameof(VendorsController.Details), "Vendors", new { id = vendorId });
            }
            return Problem();
        }

        // GET: Vendors/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Vendor == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendor
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }
            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Delete", "Vendors", $"Delete Agency") { Parent = agencyPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = editPage;
            return View(vendor);
        }

        // POST: Vendors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string VendorId)
        {
            if (_context.Vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return Problem("Entity set 'ApplicationDbContext.Vendor'  is null.");
            }
            var vendor = await _context.Vendor.FindAsync(VendorId);
            if (vendor != null)
            {
                vendor.Updated = DateTime.UtcNow;
                vendor.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Vendor.Remove(vendor);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("agency deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool VendorExists(string id)
        {
            return (_context.Vendor?.Any(e => e.VendorId == id)).GetValueOrDefault();
        }
    }
}