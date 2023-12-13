using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace risk.control.system.Controllers
{
    [Breadcrumb(" Service")]
    public class VendorServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public VendorServiceController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: VendorService
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.VendorInvestigationServiceType
                .Include(v => v.InvestigationServiceType)
                .Include(v => v.LineOfBusiness)
                .Include(v => v.PincodeServices)
                .Include(v => v.State)
                .Include(v => v.Vendor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: VendorService/Details/5
        [Breadcrumb(" Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VendorInvestigationServiceType == null)
            {
                return NotFound();
            }

            var vendorInvestigationServiceType = await _context.VendorInvestigationServiceType
                .Include(v => v.InvestigationServiceType)
                .Include(v => v.LineOfBusiness)
                .Include(v => v.PincodeServices)
                .Include(v => v.District)
                .Include(v => v.Country)
                .Include(v => v.State)
                .Include(v => v.Vendor)
                .FirstOrDefaultAsync(m => m.VendorInvestigationServiceTypeId == id);
            if (vendorInvestigationServiceType == null)
            {
                return NotFound();
            }

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = vendorInvestigationServiceType.VendorId } };
            var editPage = new MvcBreadcrumbNode("Service", "Vendors", $"Manage Services") { Parent = agencyPage, RouteValues = new { id = vendorInvestigationServiceType.VendorId } };
            ViewData["BreadcrumbNode"] = editPage;

            return View(vendorInvestigationServiceType);
        }

        // GET: VendorService/Create
        [Breadcrumb(" Add")]
        public IActionResult Create(string id)
        {
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == id);
            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name");
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            var model = new VendorInvestigationServiceType { SelectedMultiPincodeId = new List<string>(), Vendor = vendor, PincodeServices = new List<ServicedPinCode>() };

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Service", "Vendors", $"Manage Services") { Parent = agencyPage, RouteValues = new { id = id } };
            var createPage = new MvcBreadcrumbNode("Create", "VendorService", $"Add Service") { Parent = editPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = createPage;
            return View(model);
        }

        // POST: VendorService/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorInvestigationServiceType vendorInvestigationServiceType)
        {
            if (vendorInvestigationServiceType is not null)
            {
                var pincodesServiced = await _context.PinCode.Where(p => vendorInvestigationServiceType.SelectedMultiPincodeId.Contains(p.PinCodeId)).ToListAsync();
                var servicePinCodes = pincodesServiced.Select(p =>
                new ServicedPinCode
                {
                    Name = p.Name,
                    Pincode = p.Code,
                    VendorInvestigationServiceTypeId = vendorInvestigationServiceType.VendorInvestigationServiceTypeId,
                    VendorInvestigationServiceType = vendorInvestigationServiceType,
                }).ToList();
                vendorInvestigationServiceType.PincodeServices = servicePinCodes;
                vendorInvestigationServiceType.Updated = DateTime.UtcNow;
                vendorInvestigationServiceType.UpdatedBy = HttpContext.User?.Identity?.Name;
                vendorInvestigationServiceType.Created = DateTime.UtcNow;
                _context.Add(vendorInvestigationServiceType);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("service created successfully!");

                return RedirectToAction(nameof(VendorsController.Service), "Vendors", new { id = vendorInvestigationServiceType.VendorId });
            }
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", vendorInvestigationServiceType.CountryId);
            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name", vendorInvestigationServiceType.LineOfBusinessId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", vendorInvestigationServiceType.DistrictId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", vendorInvestigationServiceType.StateId);
            toastNotification.AddErrorToastMessage("Error to create vendor service!");

            return View(vendorInvestigationServiceType);
        }

        [HttpPost, ActionName("GetInvestigationServicesByLineOfBusinessId")]
        public async Task<JsonResult> GetInvestigationServicesByLineOfBusinessId(string LineOfBusinessId)
        {
            string lId;
            var services = new List<InvestigationServiceType>();
            if (!string.IsNullOrEmpty(LineOfBusinessId))
            {
                lId = LineOfBusinessId;
                services = await _context.InvestigationServiceType.Where(s => s.LineOfBusiness.LineOfBusinessId.Equals(lId)).ToListAsync();
            }
            return Json(services);
        }

        // GET: VendorService/Edit/5
        [Breadcrumb(" Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VendorInvestigationServiceType == null)
            {
                return NotFound();
            }

            var vendorInvestigationServiceType = await _context.VendorInvestigationServiceType.FindAsync(id);
            if (vendorInvestigationServiceType == null)
            {
                return NotFound();
            }
            var services = _context.VendorInvestigationServiceType
                .Include(v => v.Vendor)
                .Include(v => v.PincodeServices)
                .First(v => v.VendorInvestigationServiceTypeId == id);

            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name", vendorInvestigationServiceType.LineOfBusinessId);
            ViewData["InvestigationServiceTypeId"] = new SelectList(_context.InvestigationServiceType
                .Include(i => i.LineOfBusiness)
                .Where(i => i.LineOfBusiness.LineOfBusinessId == vendorInvestigationServiceType.LineOfBusinessId),
                "InvestigationServiceTypeId", "Name", vendorInvestigationServiceType.InvestigationServiceTypeId);

            var country = _context.Country.Where(c => c.CountryId == vendorInvestigationServiceType.CountryId).OrderBy(c => c.Name);
            var states = _context.State.Include(s => s.Country).Where(s => s.Country.CountryId == vendorInvestigationServiceType.CountryId).OrderBy(d => d.Name);
            var districts = _context.District.Include(d => d.State).Where(d => d.State.StateId == vendorInvestigationServiceType.StateId).OrderBy(d => d.Name);

            ViewData["CountryId"] = new SelectList(country, "CountryId", "Name", vendorInvestigationServiceType.CountryId);
            ViewData["StateId"] = new SelectList(states, "StateId", "Name", vendorInvestigationServiceType.StateId);
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "Name", vendorInvestigationServiceType.VendorId);
            ViewData["DistrictId"] = new SelectList(districts, "DistrictId", "Name", vendorInvestigationServiceType.DistrictId);
            ViewBag.PinCodeId = _context.PinCode.Include(p => p.District).Where(p => p.District.DistrictId == vendorInvestigationServiceType.DistrictId)
                .Select(x => new SelectListItem
                {
                    Text = x.Name + " - " + x.Code,
                    Value = x.PinCodeId.ToString()
                }).ToList();

            var selected = services.PincodeServices.Select(s => s.Pincode).ToList();
            services.SelectedMultiPincodeId = _context.PinCode.Where(p => selected.Contains(p.Code)).Select(p => p.PinCodeId).ToList();

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = services.VendorId } };
            var editPage = new MvcBreadcrumbNode("Service", "Vendors", $"Manage Services") { Parent = agencyPage, RouteValues = new { id = services.VendorId } };
            var createPage = new MvcBreadcrumbNode("Edit", "VendorService", $"Edit Service") { Parent = editPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = createPage;

            return View(services);
        }

        // POST: VendorService/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string VendorInvestigationServiceTypeId, VendorInvestigationServiceType vendorInvestigationServiceType)
        {
            if (VendorInvestigationServiceTypeId != vendorInvestigationServiceType.VendorInvestigationServiceTypeId)
            {
                return NotFound();
            }

            if (vendorInvestigationServiceType is not null)
            {
                try
                {
                    if (vendorInvestigationServiceType.SelectedMultiPincodeId.Count > 0)
                    {
                        var existingServicedPincodes = _context.ServicedPinCode.Where(s => s.VendorInvestigationServiceTypeId == vendorInvestigationServiceType.VendorInvestigationServiceTypeId);
                        _context.ServicedPinCode.RemoveRange(existingServicedPincodes);

                        var pinCodeDetails = _context.PinCode.Where(p => vendorInvestigationServiceType.SelectedMultiPincodeId.Contains(p.PinCodeId));

                        var pinCodesWithId = pinCodeDetails.Select(p => new ServicedPinCode
                        {
                            Pincode = p.Code,
                            Name = p.Name,
                            VendorInvestigationServiceTypeId = vendorInvestigationServiceType.VendorInvestigationServiceTypeId
                        }).ToList();
                        _context.ServicedPinCode.AddRange(pinCodesWithId);

                        vendorInvestigationServiceType.PincodeServices = pinCodesWithId;
                        vendorInvestigationServiceType.Updated = DateTime.UtcNow;
                        vendorInvestigationServiceType.UpdatedBy = HttpContext.User?.Identity?.Name;
                        _context.Update(vendorInvestigationServiceType);
                        await _context.SaveChangesAsync();
                        toastNotification.AddSuccessToastMessage("service updated successfully!");
                        return RedirectToAction(nameof(VendorsController.Service), "Vendors", new { id = vendorInvestigationServiceType.VendorId });
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!VendorInvestigationServiceTypeExists(vendorInvestigationServiceType.VendorInvestigationServiceTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("service edited successfully!");
                return RedirectToAction("Details", "Vendors", new { id = vendorInvestigationServiceType.VendorId });
            }
            ViewData["InvestigationServiceTypeId"] = new SelectList(_context.InvestigationServiceType, "InvestigationServiceTypeId", "Name", vendorInvestigationServiceType.InvestigationServiceTypeId);
            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name", vendorInvestigationServiceType.LineOfBusinessId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", vendorInvestigationServiceType.StateId);
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "Name", vendorInvestigationServiceType.VendorId);
            return View(vendorInvestigationServiceType);
        }

        // GET: VendorService/Delete/5
        [Breadcrumb(" Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VendorInvestigationServiceType == null)
            {
                return NotFound();
            }

            var vendorInvestigationServiceType = await _context.VendorInvestigationServiceType
                .Include(v => v.InvestigationServiceType)
                .Include(v => v.LineOfBusiness)
                .Include(v => v.PincodeServices)
                .Include(v => v.State)
                .Include(v => v.District)
                .Include(v => v.Country)
                .Include(v => v.Vendor)
                .FirstOrDefaultAsync(m => m.VendorInvestigationServiceTypeId == id);
            if (vendorInvestigationServiceType == null)
            {
                return NotFound();
            }
            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = vendorInvestigationServiceType.VendorId } };
            var editPage = new MvcBreadcrumbNode("Service", "Vendors", $"Manage Services") { Parent = agencyPage, RouteValues = new { id = vendorInvestigationServiceType.VendorId } };
            var createPage = new MvcBreadcrumbNode("Delete", "VendorService", $"Delete Service") { Parent = editPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = createPage;

            return View(vendorInvestigationServiceType);
        }

        // POST: VendorService/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VendorInvestigationServiceType == null)
            {
                return Problem("Entity set 'ApplicationDbContext.VendorInvestigationServiceType'  is null.");
            }
            var vendorInvestigationServiceType = await _context.VendorInvestigationServiceType.FindAsync(id);
            if (vendorInvestigationServiceType != null)
            {
                vendorInvestigationServiceType.Updated = DateTime.UtcNow;
                vendorInvestigationServiceType.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.VendorInvestigationServiceType.Remove(vendorInvestigationServiceType);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("service deleted successfully!");
            return RedirectToAction("Details", "Vendors", new { id = vendorInvestigationServiceType.VendorId });
        }

        private bool VendorInvestigationServiceTypeExists(string id)
        {
            return (_context.VendorInvestigationServiceType?.Any(e => e.VendorInvestigationServiceTypeId == id)).GetValueOrDefault();
        }
    }
}