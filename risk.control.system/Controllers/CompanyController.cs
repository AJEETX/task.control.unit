using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MimeKit.Encodings;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Company Profile")]
    public class CompanyController : Controller
    {
        public List<UsersViewModel> UserList;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;
        private readonly IHttpClientService httpClientService;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IToastNotification toastNotification;

        public CompanyController(ApplicationDbContext context,
            UserManager<ClientCompanyApplicationUser> userManager,
            IHttpClientService httpClientService,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IToastNotification toastNotification)
        {
            this._context = context;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.httpClientService = httpClientService;
            this.roleManager = roleManager;
            this.webHostEnvironment = webHostEnvironment;
            this.toastNotification = toastNotification;
            UserList = new List<UsersViewModel>();
        }

        public async Task<IActionResult> Index()
        {
            if (_context.ClientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var clientCompany = await _context.ClientCompany
                .Include(c => c.Country)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.ClientCompanyId == companyUser.ClientCompanyId);
            if (clientCompany == null)
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }

            return View(clientCompany);
        }

        [Breadcrumb("Edit")]
        public async Task<IActionResult> Edit()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var clientCompany = await _context.ClientCompany
                .Include(c => c.Country)
                .Include(c => c.PinCode)
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.ClientCompanyId == companyUser.ClientCompanyId);
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
            if (string.IsNullOrWhiteSpace(clientCompany.ClientCompanyId))
            {
                toastNotification.AddErrorToastMessage("client company not found!");
                return NotFound();
            }
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);
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
                        companyDocument.CopyTo(new FileStream(upload, FileMode.Create));
                        clientCompany.DocumentUrl = "/img/" + newFileName;
                        clientCompany.Document = companyDocument;
                    }
                    else
                    {
                        var existingClientCompany = await _context.ClientCompany.AsNoTracking().FirstOrDefaultAsync(c => c.ClientCompanyId == companyUser.ClientCompanyId);
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

                    var companyUsers = _context.ClientCompanyApplicationUser.Where(u => u.ClientCompanyId == companyUser.ClientCompanyId);

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
                    var currentUser = await userManager.FindByEmailAsync(userEmail);
                    await signInManager.RefreshSignInAsync(currentUser);

                    var pinCode = _context.PinCode.FirstOrDefault(p => p.PinCodeId == clientCompany.PinCodeId);

                    //var pinCodeData = await httpClientService.GetPinCodeLatLng(pinCode.Code);
                    //pinCode.Latitude = pinCodeData.FirstOrDefault()?.Lat.ToString();
                    //pinCode.Longitude = pinCodeData.FirstOrDefault()?.Lng.ToString();
                    clientCompany.Updated = DateTime.UtcNow;
                    clientCompany.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.ClientCompany.Update(clientCompany);
                    await _context.SaveChangesAsync();

                    var response = SmsService.SendSingleMessage(clientCompany.PhoneNumber, "Company edited. Domain : " + clientCompany.Email);
                }
                catch (Exception)
                {
                    toastNotification.AddErrorToastMessage("Error to edit company profile!");
                    return RedirectToAction(nameof(CompanyController.Index), "Company");
                }
                toastNotification.AddSuccessToastMessage("<i class='fas fa-building'></i> company Profile edited successfully!");
                return RedirectToAction(nameof(CompanyController.Index), "Company");
            }
            toastNotification.AddErrorToastMessage("Error to edit company profile!");
            return RedirectToAction(nameof(CompanyController.Index), "Company");
        }

        [Breadcrumb("Manage Users ")]
        public IActionResult User()
        {
            return View();
        }

        [Breadcrumb("Add User", FromAction = "User")]
        public IActionResult CreateUser()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var company = _context.ClientCompany.FirstOrDefault(v => v.ClientCompanyId == companyUser.ClientCompanyId);
            var model = new ClientCompanyApplicationUser { ClientCompany = company };
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(ClientCompanyApplicationUser user, string emailSuffix)
        {
            var userFullEmail = user.Email.Trim().ToLower() + "@" + emailSuffix;
            if (user.ProfileImage != null && user.ProfileImage.Length > 0)
            {
                string newFileName = userFullEmail;
                string fileExtension = Path.GetExtension(user.ProfileImage.FileName);
                newFileName += fileExtension;
                var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);
                user.ProfileImage.CopyTo(new FileStream(upload, FileMode.Create));

                using var dataStream = new MemoryStream();
                user.ProfileImage.CopyTo(dataStream);
                user.ProfilePicture = dataStream.ToArray();

                user.ProfilePictureUrl = "/img/" + newFileName;
            }
            //DEMO
            user.Password = Applicationsettings.Password;
            user.Email = userFullEmail;
            user.EmailConfirmed = true;
            user.UserName = userFullEmail;
            user.Mailbox = new Mailbox { Name = userFullEmail };
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = HttpContext.User?.Identity?.Name;
            IdentityResult result = await userManager.CreateAsync(user, user.Password);

            if (result.Succeeded)
            {
                if (!user.Active)
                {
                    var createdUser = await userManager.FindByEmailAsync(user.Email);
                    var lockUser = await userManager.SetLockoutEnabledAsync(createdUser, true);
                    var lockDate = await userManager.SetLockoutEndDateAsync(createdUser, DateTime.MaxValue);

                    if (lockUser.Succeeded && lockDate.Succeeded)
                    {
                        toastNotification.AddSuccessToastMessage("<i class='fas fa-user-lock'></i> User created and locked successfully!");
                        var response = SmsService.SendSingleMessage(createdUser.PhoneNumber, "User created and locked. Email : " + createdUser.Email);
                        return RedirectToAction(nameof(CompanyController.User), "Company");
                    }
                }
                else
                {
                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-plus'></i> User created successfully!");
                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "User created . Email : " + user.Email);
                    return RedirectToAction(nameof(CompanyController.User), "Company");
                }
                toastNotification.AddSuccessToastMessage("User created successfully!");
                return RedirectToAction(nameof(CompanyController.User), "Company");
            }
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
            GetCountryStateEdit(user);
            toastNotification.AddErrorToastMessage("Error to create user!");
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var company = _context.ClientCompany.FirstOrDefault(v => v.ClientCompanyId == companyUser.ClientCompanyId);
            user.ClientCompany = company;
            return View(user);
        }

        [Breadcrumb("Edit User")]
        public async Task<IActionResult> EditUser(long? userId)
        {
            if (userId == null || _context.ClientCompanyApplicationUser == null)
            {
                toastNotification.AddErrorToastMessage("company not found");
                return NotFound();
            }

            var clientCompanyApplicationUser = await _context.ClientCompanyApplicationUser.FindAsync(userId);
            if (clientCompanyApplicationUser == null)
            {
                toastNotification.AddErrorToastMessage("company not found");
                return NotFound();
            }
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            ViewBag.Show = clientCompanyApplicationUser.Email == companyUser.Email ? false : true;
            var clientCompany = _context.ClientCompany.FirstOrDefault(v => v.ClientCompanyId == clientCompanyApplicationUser.ClientCompanyId);

            if (clientCompany == null)
            {
                toastNotification.AddErrorToastMessage("company not found");
                return NotFound();
            }
            clientCompanyApplicationUser.ClientCompany = clientCompany;

            var country = _context.Country.Where(c => c.CountryId == clientCompanyApplicationUser.CountryId);
            var relatedStates = _context.State.Include(s => s.Country).Where(s => s.Country.CountryId == clientCompanyApplicationUser.CountryId).OrderBy(d => d.Name);
            var districts = _context.District.Include(d => d.State).Where(d => d.State.StateId == clientCompanyApplicationUser.StateId).OrderBy(d => d.Name);
            var pincodes = _context.PinCode.Include(d => d.District).Where(d => d.District.DistrictId == clientCompanyApplicationUser.DistrictId).OrderBy(d => d.Name);

            ViewData["CountryId"] = new SelectList(country.OrderBy(c => c.Name), "CountryId", "Name", clientCompanyApplicationUser.CountryId);
            ViewData["StateId"] = new SelectList(relatedStates, "StateId", "Name", clientCompanyApplicationUser.StateId);
            ViewData["DistrictId"] = new SelectList(districts, "DistrictId", "Name", clientCompanyApplicationUser.DistrictId);
            ViewData["PinCodeId"] = new SelectList(pincodes, "PinCodeId", "Code", clientCompanyApplicationUser.PinCodeId);

            var companyPage = new MvcBreadcrumbNode("Index", "Company", "Company");
            var usersPage = new MvcBreadcrumbNode("User", "Company", "Users") { Parent = companyPage };
            var userEditPage = new MvcBreadcrumbNode("UserEdit", "Company", $"Edit User") { Parent = usersPage, RouteValues = new { userid = userId } };
            ViewData["BreadcrumbNode"] = userEditPage;

            return View(clientCompanyApplicationUser);
        }

        // POST: ClientCompanyApplicationUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, ClientCompanyApplicationUser applicationUser)
        {
            if (id != applicationUser.Id.ToString())
            {
                toastNotification.AddErrorToastMessage("company not found!");
                return RedirectToAction(nameof(CompanyController.User), "Company");
            }

            if (applicationUser is not null)
            {
                try
                {
                    var user = await userManager.FindByIdAsync(id);
                    if (applicationUser?.ProfileImage != null && applicationUser.ProfileImage.Length > 0)
                    {
                        string newFileName = applicationUser.Email + Guid.NewGuid().ToString();
                        string fileExtension = Path.GetExtension(applicationUser.ProfileImage.FileName);
                        newFileName += fileExtension;
                        var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);
                        applicationUser.ProfileImage.CopyTo(new FileStream(upload, FileMode.Create));
                        applicationUser.ProfilePictureUrl = "/img/" + newFileName;
                        using var dataStream = new MemoryStream();
                        applicationUser.ProfileImage.CopyTo(dataStream);
                        applicationUser.ProfilePicture = dataStream.ToArray();
                    }

                    if (user != null)
                    {
                        user.ProfileImage = applicationUser?.ProfileImage ?? user.ProfileImage;
                        user.ProfilePictureUrl = applicationUser?.ProfilePictureUrl ?? user.ProfilePictureUrl;
                        user.PhoneNumber = applicationUser?.PhoneNumber ?? user.PhoneNumber;
                        user.FirstName = applicationUser?.FirstName;
                        user.LastName = applicationUser?.LastName;
                        if (!string.IsNullOrWhiteSpace(applicationUser?.Password))
                        {
                            user.Password = applicationUser.Password;
                        }
                        user.Active = applicationUser.Active;
                        user.Addressline = applicationUser.Addressline;
                        user.Country = applicationUser.Country;
                        user.CountryId = applicationUser.CountryId;
                        user.State = applicationUser.State;
                        user.StateId = applicationUser.StateId;
                        user.PinCode = applicationUser.PinCode;
                        user.PinCodeId = applicationUser.PinCodeId;
                        user.Updated = DateTime.UtcNow;
                        user.Comments = applicationUser.Comments;
                        user.PhoneNumber = applicationUser.PhoneNumber;
                        user.UpdatedBy = HttpContext.User?.Identity?.Name;
                        user.SecurityStamp = DateTime.UtcNow.ToString();
                        var result = await userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            if (!user.Active)
                            {
                                var createdUser = await userManager.FindByEmailAsync(user.Email);
                                var lockUser = await userManager.SetLockoutEnabledAsync(createdUser, true);
                                var lockDate = await userManager.SetLockoutEndDateAsync(createdUser, DateTime.MaxValue);

                                if (lockUser.Succeeded && lockDate.Succeeded)
                                {
                                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-lock'></i> User created and locked successfully!");
                                    var response = SmsService.SendSingleMessage(createdUser.PhoneNumber, "User created and locked. Email : " + createdUser.Email);
                                    return RedirectToAction(nameof(CompanyController.User), "Company");
                                }
                            }
                            else
                            {
                                var createdUser = await userManager.FindByEmailAsync(user.Email);
                                var lockUser = await userManager.SetLockoutEnabledAsync(createdUser, true);
                                var lockDate = await userManager.SetLockoutEndDateAsync(user, DateTime.Now);

                                if (lockUser.Succeeded && lockDate.Succeeded)
                                {
                                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-check'></i> User edited and unlocked successfully!");
                                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "User created . Email : " + user.Email);
                                    return RedirectToAction(nameof(CompanyController.User), "Company");
                                }
                            }
                        }
                        toastNotification.AddErrorToastMessage("Error !!. The user can't be edited!");
                        Errors(result);
                        return RedirectToAction(nameof(CompanyController.User), "Company");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorApplicationUserExists(applicationUser.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            toastNotification.AddErrorToastMessage("Error to create Company user!");
            return RedirectToAction(nameof(CompanyController.User), "Company");
        }

        [Breadcrumb("Available Agencies")]
        public async Task<IActionResult> AvailableVendors()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AvailableVendors(List<string> vendors)
        {
            if (vendors is not null && vendors.Count > 0)
            {
                var userEmail = HttpContext.User?.Identity?.Name;
                var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);
                var company = _context.ClientCompany
               .Include(c => c.CompanyApplicationUser)
               .Include(c => c.EmpanelledVendors)
               .FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

                if (company != null)
                {
                    var vendors2Empanel = _context.Vendor.AsNoTracking().Where(v => vendors.Contains(v.VendorId));
                    company.EmpanelledVendors.AddRange(vendors2Empanel.ToList());

                    company.Updated = DateTime.UtcNow;
                    company.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.ClientCompany.Update(company);
                    var savedRows = await _context.SaveChangesAsync();

                    foreach (var vendor2Empanel in vendors2Empanel)
                    {
                        vendor2Empanel.Clients.Add(company);
                    }

                    toastNotification.AddSuccessToastMessage("Agency(s) empanel successful!");
                    try
                    {
                        return RedirectToAction("AvailableVendors");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            return Problem();
        }

        [Breadcrumb("Empanelled Agencies")]
        public async Task<IActionResult> EmpanelledVendors()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmpanelledVendors(List<string> vendors)
        {
            if (vendors is not null && vendors.Count() > 0)
            {
                var userEmail = HttpContext.User?.Identity?.Name;
                var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

                var company = _context.ClientCompany
                    .Include(c => c.CompanyApplicationUser)
                    .Include(c => c.EmpanelledVendors)
                    .FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

                if (company != null)
                {
                    var empanelledVendors2Depanel = _context.Vendor.Include(v => v.Clients).AsNoTracking().Where(v => vendors.Contains(v.VendorId));
                    foreach (var empanelledVendor2Depanel in empanelledVendors2Depanel)
                    {
                        var empanelled = company.EmpanelledVendors.FirstOrDefault(v => v.VendorId == empanelledVendor2Depanel.VendorId);
                        company.EmpanelledVendors.Remove(empanelled);
                        var clientCompany = empanelledVendor2Depanel.Clients.FirstOrDefault(c => c.ClientCompanyId == company.ClientCompanyId);

                        empanelledVendor2Depanel.Clients.Remove(clientCompany);
                    }
                    company.Updated = DateTime.UtcNow;
                    company.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.ClientCompany.Update(company);
                    var savedRows = await _context.SaveChangesAsync();
                    toastNotification.AddSuccessToastMessage("Agency(s) depanel sucessful!");
                    try
                    {
                        if (savedRows > 0)
                        {
                            return RedirectToAction("EmpanelledVendors");
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
            return Problem();
        }

        [Breadcrumb("Agency Detail", FromAction = "AvailableVendors")]
        public async Task<IActionResult> VendorDetail(string id, string backurl)
        {
            if (id == null || _context.Vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return RedirectToAction(nameof(CompanyController.User), "Company");
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
                toastNotification.AddErrorToastMessage("agency not found!");
                return RedirectToAction(nameof(CompanyController.User), "Company");
            }
            ViewBag.Backurl = backurl;

            return View(vendor);
        }

        [Breadcrumb("Agency Detail", FromAction = "EmpanelledVendors")]
        public async Task<IActionResult> VendorDetails(string id, string backurl)
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
            ViewBag.Backurl = backurl;

            return View(vendor);
        }

        [Breadcrumb("Edit Role", FromAction = "User")]
        public async Task<IActionResult> UserRoles(string userId)
        {
            var userRoles = new List<CompanyUserRoleViewModel>();
            //ViewBag.userId = userId;
            ClientCompanyApplicationUser user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return NotFound();
            }
            //ViewBag.UserName = user.UserName;
            foreach (var role in roleManager.Roles.Where(r =>
                r.Name.Contains(AppRoles.CompanyAdmin.ToString()) ||
                r.Name.Contains(AppRoles.Creator.ToString()) ||
                r.Name.Contains(AppRoles.Assigner.ToString()) ||
                r.Name.Contains(AppRoles.Assessor.ToString())))
            {
                var userRoleViewModel = new CompanyUserRoleViewModel
                {
                    RoleId = role.Id.ToString(),
                    RoleName = role?.Name
                };
                if (await userManager.IsInRoleAsync(user, role?.Name))
                {
                    userRoleViewModel.Selected = true;
                }
                else
                {
                    userRoleViewModel.Selected = false;
                }
                userRoles.Add(userRoleViewModel);
            }
            var model = new CompanyUserRolesViewModel
            {
                UserId = userId,
                CompanyId = user.ClientCompanyId,
                UserName = user.UserName,
                CompanyUserRoleViewModel = userRoles
            };

            var companyPage = new MvcBreadcrumbNode("Index", "Company", "Company");
            var usersPage = new MvcBreadcrumbNode("User", "Company", "Users") { Parent = companyPage };
            var userPage = new MvcBreadcrumbNode("EditUser", "Company", $"User") { Parent = usersPage, RouteValues = new { userid = userId } };
            var userRolePage = new MvcBreadcrumbNode("UserRoles", "Company", $"Edit Role") { Parent = usersPage, RouteValues = new { userid = userId } };
            ViewData["BreadcrumbNode"] = userRolePage;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string userId, CompanyUserRolesViewModel model)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return RedirectToAction(nameof(CompanyController.User), "Company");
            }
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = HttpContext.User?.Identity?.Name;
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            result = await userManager.AddToRolesAsync(user, model.CompanyUserRoleViewModel.Where(x => x.Selected).Select(y => y.RoleName));
            var currentUser = await userManager.GetUserAsync(HttpContext.User);
            await signInManager.RefreshSignInAsync(currentUser);
            var response = SmsService.SendSingleMessage(user.PhoneNumber, "User role edited . Email : " + user.Email);

            toastNotification.AddSuccessToastMessage("<i class='fas fa-user-cog'></i>  User role(s) updated successfully!");
            return RedirectToAction(nameof(CompanyController.User));
        }

        private bool VendorApplicationUserExists(long id)
        {
            return (_context.VendorApplicationUser?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        private async Task<List<string>> GetUserRoles(ClientCompanyApplicationUser user)
        {
            return new List<string>(await userManager.GetRolesAsync(user));
        }

        private bool ClientCompanyExists(string id)
        {
            return (_context.ClientCompany?.Any(e => e.ClientCompanyId == id)).GetValueOrDefault();
        }

        private void GetCountryStateEdit(ClientCompanyApplicationUser? user)
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", user?.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", user?.DistrictId);
            ViewData["StateId"] = new SelectList(_context.State.Where(s => s.CountryId == user.CountryId), "StateId", "Name", user?.StateId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode.Where(s => s.StateId == user.StateId), "PinCodeId", "Name", user?.PinCodeId);
        }
    }
}