using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Company ")]
    public class ClientCompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IToastNotification toastNotification;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;

        public ClientCompanyController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ClientCompanyApplicationUser> userManager,
            IToastNotification toastNotification)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
            this.toastNotification = toastNotification;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        // GET: ClientCompanies/Create
        [Breadcrumb("Create")]
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            return View();
        }

        // POST: ClientCompanies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(ClientCompany clientCompany, string domainAddress, string mailAddress)
        {
            if (clientCompany is not null)
            {
                Domain domainData = (Domain)Enum.Parse(typeof(Domain), domainAddress, true);

                clientCompany.Email = mailAddress.ToLower() + domainData.GetEnumDisplayName();
                IFormFile? companyDocument = Request.Form?.Files?.FirstOrDefault();
                if (companyDocument is not null)
                {
                    string newFileName = clientCompany.Email;
                    string fileExtension = Path.GetExtension(companyDocument.FileName);
                    newFileName += fileExtension;
                    var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);
                    companyDocument.CopyTo(new FileStream(upload, FileMode.Create));
                    clientCompany.DocumentUrl = "/img/" + newFileName;

                    using var dataStream = new MemoryStream();
                    companyDocument.CopyTo(dataStream);
                    clientCompany.DocumentImage = dataStream.ToArray();
                }

                var response = SmsService.SendSingleMessage(clientCompany.PhoneNumber, "Company account created. Domain : " + clientCompany.Email);

                clientCompany.Updated = DateTime.UtcNow;
                clientCompany.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Add(clientCompany);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("Company created successfully!");
                return RedirectToAction(nameof(Index));
            }
            toastNotification.AddErrorToastMessage("Company not found!");
            return Problem();
        }

        // GET: ClientCompanies/Delete/5
        [Breadcrumb("Delete ")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ClientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            var clientCompany = await _context.ClientCompany
                .Include(c => c.Country)
                .Include(c => c.PinCode)
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.ClientCompanyId == id);
            if (clientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            return View(clientCompany);
        }

        // POST: ClientCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ClientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return Problem("Entity set 'ApplicationDbContext.ClientCompany'  is null.");
            }
            var clientCompany = await _context.ClientCompany.FindAsync(id);
            if (clientCompany != null)
            {
                clientCompany.Updated = DateTime.UtcNow;
                clientCompany.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.ClientCompany.Remove(clientCompany);
            }

            await _context.SaveChangesAsync();

            var response = SmsService.SendSingleMessage(clientCompany.PhoneNumber, "Company account deleted. Domain : " + clientCompany.Email);

            toastNotification.AddSuccessToastMessage("client company deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        // GET: ClientCompanies/Details/5
        [Breadcrumb(" Profile")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ClientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            var clientCompany = await _context.ClientCompany
                .Include(c => c.Country)
                .Include(c => c.PinCode)
                .Include(c => c.State)
                .Include(c => c.District)
                .FirstOrDefaultAsync(m => m.ClientCompanyId == id);
            if (clientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            return View(clientCompany);
        }

        // GET: ClientCompanies/Edit/5
        [Breadcrumb(title: "Edit ")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ClientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            var clientCompany = await _context.ClientCompany.FindAsync(id);
            if (clientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            var country = _context.Country.Where(c => c.CountryId == clientCompany.CountryId);
            var relatedStates = _context.State.Include(s => s.Country).Where(s => s.Country.CountryId == clientCompany.CountryId).OrderBy(d => d.Name);
            var districts = _context.District.Include(d => d.State).Where(d => d.State.StateId == clientCompany.StateId).OrderBy(d => d.Name);
            var pincodes = _context.PinCode.Include(d => d.District).Where(d => d.District.DistrictId == clientCompany.DistrictId).OrderBy(d => d.Name);

            ViewData["CountryId"] = new SelectList(country.OrderBy(c => c.Name), "CountryId", "Name", clientCompany.CountryId);
            ViewData["StateId"] = new SelectList(relatedStates, "StateId", "Name", clientCompany.StateId);
            ViewData["DistrictId"] = new SelectList(districts, "DistrictId", "Name", clientCompany.DistrictId);
            ViewData["PinCodeId"] = new SelectList(pincodes, "PinCodeId", "Code", clientCompany.PinCodeId);

            return View(clientCompany);
        }

        // POST: ClientCompanies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientCompany clientCompany)
        {
            if (clientCompany is null || string.IsNullOrWhiteSpace(clientCompany.ClientCompanyId))
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return RedirectToAction("Index");
            }

            if (clientCompany is not null)
            {
                try
                {
                    IFormFile? companyDocument = Request.Form?.Files?.FirstOrDefault();
                    if (companyDocument is not null)
                    {
                        string newFileName = clientCompany.Email + Guid.NewGuid().ToString();
                        string fileExtension = Path.GetExtension(companyDocument.FileName);
                        newFileName += fileExtension;
                        var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);

                        using var dataStream = new MemoryStream();
                        companyDocument.CopyTo(dataStream);
                        clientCompany.DocumentImage = dataStream.ToArray();
                        companyDocument.CopyTo(new FileStream(upload, FileMode.Create));
                        clientCompany.DocumentUrl = "/img/" + newFileName;
                    }
                    else
                    {
                        var existingClientCompany = await _context.ClientCompany.AsNoTracking().FirstOrDefaultAsync(c => c.ClientCompanyId == clientCompany.ClientCompanyId);
                        if (existingClientCompany.DocumentUrl != null)
                        {
                            clientCompany.DocumentImage = existingClientCompany.DocumentImage;
                            clientCompany.DocumentUrl = existingClientCompany.DocumentUrl;
                        }
                    }

                    var assignerRole = roleManager.Roles.FirstOrDefault(r =>
                            r.Name.Contains(AppRoles.Assigner.ToString()));
                    var creatorRole = roleManager.Roles.FirstOrDefault(r =>
                            r.Name.Contains(AppRoles.Creator.ToString()));

                    var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == clientCompany.ClientCompanyId);

                    string currentOwner = string.Empty;
                    foreach (var companyuser in companyUsers)
                    {
                        var isCreator = await userManager.IsInRoleAsync(companyuser, creatorRole?.Name);
                        if (isCreator)
                        {
                            currentOwner = companyuser.Email;

                            ClientCompanyApplicationUser user = await userManager.FindByEmailAsync(currentOwner);

                            if (clientCompany.Auto)
                            {
                                var result = await userManager.AddToRoleAsync(user, assignerRole.Name);
                            }
                            else
                            {
                                var result = await userManager.RemoveFromRoleAsync(user, assignerRole.Name);
                            }
                        }
                    }

                    clientCompany.Updated = DateTime.UtcNow;
                    clientCompany.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.ClientCompany.Update(clientCompany);
                    await _context.SaveChangesAsync();

                    var response = SmsService.SendSingleMessage(clientCompany.PhoneNumber, "Company account edited. Domain : " + clientCompany.Email);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientCompanyExists(clientCompany.ClientCompanyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("Company edited successfully!");
                return RedirectToAction(nameof(ClientCompanyController.Details), "ClientCompany", new { id = clientCompany.ClientCompanyId });
            }
            toastNotification.AddErrorToastMessage("Error to edit Company!");
            return Problem();
        }

        // GET: ClientCompanies
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? currentPage, int pageSize = 10)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = string.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            if (searchString != null)
            {
                currentPage = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var applicationDbContext = _context.ClientCompany.Include(c => c.Country).Include(c => c.PinCode).Include(c => c.State).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                applicationDbContext = applicationDbContext.Where(a =>
                a.Name.ToLower().Contains(searchString.Trim().ToLower()) ||
                a.Code.ToLower().Contains(searchString.Trim().ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    applicationDbContext = applicationDbContext.OrderByDescending(s => s.Name);
                    break;

                case "code_desc":
                    applicationDbContext = applicationDbContext.OrderByDescending(s => s.Code);
                    break;

                default:
                    applicationDbContext.OrderByDescending(s => s.Name);
                    break;
            }
            int pageNumber = (currentPage ?? 1);
            ViewBag.TotalPages = (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.ShowPrevious = pageNumber > 1;
            ViewBag.ShowNext = pageNumber < (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));
            ViewBag.ShowFirst = pageNumber != 1;
            ViewBag.ShowLast = pageNumber != (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));

            var applicationDbContextResult = await applicationDbContext.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(applicationDbContextResult);
        }

        [Breadcrumb("Empanelled Agency(s)")]
        [HttpGet]
        public async Task<IActionResult> EmpanelledVendors(string id, string sortOrder, string currentFilter, string searchString, int? currentPage, int pageSize = 10)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = string.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            if (searchString != null)
            {
                currentPage = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var applicationDbContext = _context.Vendor
                .Where(v => v.Clients.Any(c => c.ClientCompanyId == id))
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.District)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.LineOfBusiness)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.InvestigationServiceType)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.PincodeServices)

                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                applicationDbContext = applicationDbContext.Where(a =>
                    a.Name.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Addressline.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.BankAccountNumber.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.BankName.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Branch.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.City.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Email.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.IFSCCode.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.PhoneNumber.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.District.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.LineOfBusiness.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.InvestigationServiceType.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.PincodeServices.Any(p => p.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.PhoneNumber.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Code.ToLower().Contains(searchString.Trim().ToLower())));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    applicationDbContext = applicationDbContext.OrderByDescending(s => s.Name);
                    break;

                case "code_desc":
                    applicationDbContext = applicationDbContext.OrderByDescending(s => s.Code);
                    break;

                default:
                    applicationDbContext.OrderByDescending(s => s.Name);
                    break;
            }
            int pageNumber = (currentPage ?? 1);
            ViewBag.TotalPages = (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.ShowPrevious = pageNumber > 1;
            ViewBag.ShowNext = pageNumber < (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));
            ViewBag.ShowFirst = pageNumber != 1;
            ViewBag.ShowLast = pageNumber != (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));

            var applicationDbContextResult = await applicationDbContext.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.CompanyId = id;

            return View(applicationDbContextResult);
        }

        [Breadcrumb("Available Agency(s)")]
        [HttpGet]
        public async Task<IActionResult> AvailableVendors(string id, string sortOrder, string currentFilter, string searchString, int? currentPage, int pageSize = 10)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = string.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            if (searchString != null)
            {
                currentPage = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            //var vendorUsers = _context.VendorApplicationUser.Include(u => u.Vendor).Where(u => u.Vendor.ClientCompanyId == id).ToList();
            //List<string> userVendorids = new List<string>();
            //if (vendorUsers is not null && vendorUsers.Count > 0)
            //{
            //    userVendorids = vendorUsers.Select(u => u.VendorId).ToList();
            //}

            var applicationDbContext = _context.Vendor
                .Where(v => !v.Clients.Any(c => c.ClientCompanyId == id)
                //&& userVendorids.Contains(v.VendorId)
                && (v.VendorInvestigationServiceTypes != null) && v.VendorInvestigationServiceTypes.Count > 0)
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.District)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.LineOfBusiness)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.InvestigationServiceType)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.PincodeServices)

                .AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                applicationDbContext = applicationDbContext.Where(a =>
                    a.Name.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Addressline.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.BankAccountNumber.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.BankName.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Branch.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.City.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Email.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.IFSCCode.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.PhoneNumber.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.District.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.LineOfBusiness.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.InvestigationServiceType.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.VendorInvestigationServiceTypes.Any(v => v.PincodeServices.Any(p => p.Name.Contains(searchString.Trim().ToLower()) || v.Price.ToString().Contains(searchString.Trim().ToLower())) ||
                    a.PhoneNumber.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.Code.ToLower().Contains(searchString.Trim().ToLower())));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    applicationDbContext = applicationDbContext.OrderByDescending(s => s.Name);
                    break;

                case "code_desc":
                    applicationDbContext = applicationDbContext.OrderByDescending(s => s.Code);
                    break;

                default:
                    applicationDbContext.OrderByDescending(s => s.Name);
                    break;
            }
            int pageNumber = (currentPage ?? 1);
            ViewBag.TotalPages = (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.ShowPrevious = pageNumber > 1;
            ViewBag.ShowNext = pageNumber < (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));
            ViewBag.ShowFirst = pageNumber != 1;
            ViewBag.ShowLast = pageNumber != (int)Math.Ceiling(decimal.Divide(applicationDbContext.Count(), pageSize));

            var applicationDbContextResult = await applicationDbContext.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.CompanyId = id;
            return View(applicationDbContextResult);
        }

        [HttpPost]
        public async Task<IActionResult> AvailableVendors(string id, List<string> vendors)
        {
            if (vendors is not null && vendors.Count > 0)
            {
                var company = await _context.ClientCompany.FindAsync(id);
                if (company != null)
                {
                    var empanelledVendors = _context.Vendor.Where(v => vendors.Contains(v.VendorId))
                    .Include(v => v.Country)
                    .Include(v => v.PinCode)
                    .Include(v => v.State)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.District)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.LineOfBusiness)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.InvestigationServiceType)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.PincodeServices);

                    company.EmpanelledVendors.AddRange(empanelledVendors);

                    foreach (var empanelledVendor in empanelledVendors)
                    {
                        empanelledVendor.Clients.Add(company);
                        _context.Vendor.Update(empanelledVendor);
                    }
                    company.Updated = DateTime.UtcNow;
                    company.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.ClientCompany.Update(company);
                    var savedRows = await _context.SaveChangesAsync();
                    toastNotification.AddSuccessToastMessage("Vendor(s) empanel successful!");
                    try
                    {
                        return RedirectToAction("Details", new { id = company.ClientCompanyId });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            ViewBag.CompanyId = id;
            return Problem();
        }

        [HttpPost]
        public async Task<IActionResult> EmpanelledVendors(string id, List<string> vendors)
        {
            if (vendors is not null && vendors.Count() > 0)
            {
                var company = await _context.ClientCompany.FindAsync(id);

                if (company != null)
                {
                    var empanelledVendors = _context.Vendor.Where(v => vendors.Contains(v.VendorId))
                    .Where(v => v.Clients.Any(c => c.ClientCompanyId == id))
                    .Include(v => v.Country)
                    .Include(v => v.PinCode)
                    .Include(v => v.State)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.District)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.LineOfBusiness)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.InvestigationServiceType)
                    .Include(v => v.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.PincodeServices);
                    foreach (var v in empanelledVendors)
                    {
                        company.EmpanelledVendors.Remove(v);
                        v.Clients.Remove(company);
                        _context.Vendor.Update(v);
                    }
                    _context.ClientCompany.Update(company);
                    company.Updated = DateTime.UtcNow;
                    company.UpdatedBy = HttpContext.User?.Identity?.Name;
                    var savedRows = await _context.SaveChangesAsync();
                    toastNotification.AddSuccessToastMessage("Vendor(s) depanel sucessful!");
                    try
                    {
                        if (savedRows > 0)
                        {
                            return RedirectToAction("Details", new { id = company.ClientCompanyId });
                        }
                        else
                        {
                            return Problem();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            ViewBag.CompanyId = id;
            return Problem();
        }

        [Breadcrumb("Agency Detail")]
        public async Task<IActionResult> VendorDetail(string companyId, string id, string backurl)
        {
            if (id == null || _context.Vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return NotFound();
            }

            var vendor = await _context.Vendor
                .Include(v => v.Country)
                .Include(v => v.PinCode)
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
            ViewBag.CompanyId = companyId;
            ViewBag.Backurl = backurl;

            return View(vendor);
        }

        private bool ClientCompanyExists(string id)
        {
            return (_context.ClientCompany?.Any(e => e.ClientCompanyId == id)).GetValueOrDefault();
        }
    }
}