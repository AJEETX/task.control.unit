using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Manage Agency ")]
    public class AgencyController : Controller
    {
        public List<UsersViewModel> UserList;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<VendorApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IDashboardService dashboardService;
        private readonly IToastNotification toastNotification;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AgencyController(ApplicationDbContext context,
            UserManager<VendorApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IDashboardService dashboardService,
            IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dashboardService = dashboardService;
            this.toastNotification = toastNotification;
            this.webHostEnvironment = webHostEnvironment;
            UserList = new List<UsersViewModel>();
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);

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
                .FirstOrDefaultAsync(m => m.VendorId == vendorUser.VendorId);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // GET: Vendors/Edit/5
        [Breadcrumb("Edit Agency", FromAction = "Index")]
        public async Task<IActionResult> Edit()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var vendor = await _context.Vendor.FindAsync(vendorUser.VendorId);
            if (vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return RedirectToAction(nameof(AgencyController.Index), "Agency");
            }

            var country = _context.Country.Where(c => c.CountryId == vendor.CountryId);
            var relatedStates = _context.State.Include(s => s.Country).Where(s => s.Country.CountryId == vendor.CountryId).OrderBy(d => d.Name);
            var districts = _context.District.Include(d => d.State).Where(d => d.State.StateId == vendor.StateId).OrderBy(d => d.Name);
            var pincodes = _context.PinCode.Include(d => d.District).Where(d => d.District.DistrictId == vendor.DistrictId).OrderBy(d => d.Name);

            ViewData["CountryId"] = new SelectList(country.OrderBy(c => c.Name), "CountryId", "Name", vendor.CountryId);
            ViewData["StateId"] = new SelectList(relatedStates, "StateId", "Name", vendor.StateId);
            ViewData["DistrictId"] = new SelectList(districts, "DistrictId", "Name", vendor.DistrictId);
            ViewData["PinCodeId"] = new SelectList(pincodes, "PinCodeId", "Code", vendor.PinCodeId);

            return View(vendor);
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Vendor vendor)
        {
            if (vendor == null || string.IsNullOrWhiteSpace(vendor.VendorId))
            {
                toastNotification.AddErrorToastMessage("agency not found!");
                return RedirectToAction(nameof(AgencyController.Index), "Agency");
            }
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);

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
                        var existingVendor = await _context.Vendor.AsNoTracking().FirstOrDefaultAsync(c => c.VendorId == vendorUser.VendorId);
                        if (existingVendor.DocumentImage != null)
                        {
                            vendor.DocumentImage = existingVendor.DocumentImage;
                            vendor.DocumentUrl = existingVendor.DocumentUrl;
                        }
                    }
                    vendor.Updated = DateTime.UtcNow;
                    vendor.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Vendor.Update(vendor);
                    await _context.SaveChangesAsync();

                    var response = SmsService.SendSingleMessage(vendor.PhoneNumber, "Agency account created. Domain : " + vendor.Email);
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
                return RedirectToAction(nameof(AgencyController.Index), "Agency");
            }
            return Problem();
        }

        [Breadcrumb("Manage Users ")]
        public async Task<IActionResult> User()
        {
            var userEmail = HttpContext.User?.Identity?.Name;

            return View();
        }

        [Breadcrumb("Add User", FromAction = "User")]
        public IActionResult CreateUser()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == vendorUser.VendorId);
            var model = new VendorApplicationUser { Vendor = vendor };
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(VendorApplicationUser user, string emailSuffix)
        {
            var userFullEmail = user.Email.Trim().ToLower() + "@" + emailSuffix;
            if (user.ProfileImage != null && user.ProfileImage.Length > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
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
                        var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user created and locked. Email : " + user.Email);

                        return RedirectToAction(nameof(AgencyController.User), "Agency");
                    }
                }
                else
                {
                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-plus'></i> User created successfully!");
                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user created. Email : " + user.Email);
                    return RedirectToAction(nameof(AgencyController.User), "Agency");
                }
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            GetCountryStateEdit(user);
            toastNotification.AddErrorToastMessage("Error to create user!");
            return View(user);
        }

        [Breadcrumb("Edit User", FromAction = "User")]
        public async Task<IActionResult> EditUser(long? userId)
        {
            if (userId == null || _context.VendorApplicationUser == null)
            {
                toastNotification.AddErrorToastMessage("agency not found");
                return RedirectToAction(nameof(AgencyController.User), "Agency");
            }

            var vendorApplicationUser = await _context.VendorApplicationUser.FindAsync(userId);
            if (vendorApplicationUser == null)
            {
                toastNotification.AddErrorToastMessage("agency not found");
                return RedirectToAction(nameof(AgencyController.User), "Agency");
            }
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            ViewBag.Show = vendorApplicationUser.Email == vendorUser.Email ? false : true;

            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == vendorApplicationUser.VendorId);

            if (vendor == null)
            {
                toastNotification.AddErrorToastMessage("agency not found");
                return RedirectToAction(nameof(AgencyController.User), "Agency");
            }
            vendorApplicationUser.Vendor = vendor;

            var country = _context.Country.Where(c => c.CountryId == vendorApplicationUser.CountryId);
            var relatedStates = _context.State.Include(s => s.Country).Where(s => s.Country.CountryId == vendorApplicationUser.CountryId).OrderBy(d => d.Name);
            var districts = _context.District.Include(d => d.State).Where(d => d.State.StateId == vendorApplicationUser.StateId).OrderBy(d => d.Name);
            var pincodes = _context.PinCode.Include(d => d.District).Where(d => d.District.DistrictId == vendorApplicationUser.DistrictId).OrderBy(d => d.Name);

            ViewData["CountryId"] = new SelectList(country.OrderBy(c => c.Name), "CountryId", "Name", vendorApplicationUser.CountryId);
            ViewData["StateId"] = new SelectList(relatedStates, "StateId", "Name", vendorApplicationUser.StateId);
            ViewData["DistrictId"] = new SelectList(districts, "DistrictId", "Name", vendorApplicationUser.DistrictId);
            ViewData["PinCodeId"] = new SelectList(pincodes, "PinCodeId", "Code", vendorApplicationUser.PinCodeId);

            return View(vendorApplicationUser);
        }

        // POST: ClientCompanyApplicationUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, VendorApplicationUser applicationUser)
        {
            if (id != applicationUser.Id.ToString())
            {
                toastNotification.AddErrorToastMessage("Err !!!");
                return RedirectToAction(nameof(AgencyController.User), "Agency");
            }

            if (applicationUser is not null)
            {
                try
                {
                    var user = await userManager.FindByIdAsync(id);
                    if (applicationUser?.ProfileImage != null && applicationUser.ProfileImage.Length > 0)
                    {
                        string newFileName = Guid.NewGuid().ToString();
                        string fileExtension = Path.GetExtension(applicationUser.ProfileImage.FileName);
                        newFileName += fileExtension;
                        var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);
                        applicationUser.ProfileImage.CopyTo(new FileStream(upload, FileMode.Create));
                        using var dataStream = new MemoryStream();
                        applicationUser.ProfileImage.CopyTo(dataStream);
                        applicationUser.ProfilePicture = dataStream.ToArray();
                        applicationUser.ProfilePictureUrl = "/img/" + newFileName;
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
                        user.Addressline = applicationUser.Addressline;
                        user.Active = applicationUser.Active;
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
                                var lockDate = await userManager.SetLockoutEndDateAsync(user, DateTime.MaxValue);

                                if (lockUser.Succeeded && lockDate.Succeeded)
                                {
                                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-lock'></i> User edited and locked successfully!");
                                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user edited and locked. Email : " + user.Email);
                                    return RedirectToAction(nameof(AgencyController.User), "Agency");
                                }
                            }
                            if (user.Active)
                            {
                                var createdUser = await userManager.FindByEmailAsync(user.Email);
                                var lockUser = await userManager.SetLockoutEnabledAsync(createdUser, false);
                                var lockDate = await userManager.SetLockoutEndDateAsync(user, DateTime.Now);

                                if (lockUser.Succeeded && lockDate.Succeeded)
                                {
                                    toastNotification.AddSuccessToastMessage("User edited and unlocked successfully!");
                                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user edited and unlocked. Email : " + user.Email);
                                    return RedirectToAction(nameof(AgencyController.User), "Agency");
                                }
                            }
                            toastNotification.AddSuccessToastMessage("<i class='fas fa-user-check'></i> Agency user edited successfully!");
                            return RedirectToAction(nameof(AgencyController.User), "Agency");
                        }
                        toastNotification.AddErrorToastMessage("Error !!. The user can't be edited!");
                        Errors(result);
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

            toastNotification.AddErrorToastMessage("Error to create Agency user!");
            return RedirectToAction(nameof(AgencyController.User), "Agency");
        }

        [Breadcrumb("Edit Role", FromAction = "User")]
        public async Task<IActionResult> UserRoles(string userId)
        {
            var userRoles = new List<VendorUserRoleViewModel>();
            //ViewBag.userId = userId;
            VendorApplicationUser user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return RedirectToAction(nameof(AgencyController.User), "Agency");
            }
            //ViewBag.UserName = user.UserName;
            foreach (var role in roleManager.Roles.Where(r =>
                r.Name.Contains(AppRoles.AgencyAdmin.ToString()) ||
                r.Name.Contains(AppRoles.Supervisor.ToString()) ||
                r.Name.Contains(AppRoles.Agent.ToString())))
            {
                var userRoleViewModel = new VendorUserRoleViewModel
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
            var model = new VendorUserRolesViewModel
            {
                UserId = userId,
                VendorId = user.VendorId,
                UserName = user.UserName,
                VendorUserRoleViewModel = userRoles
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string userId, VendorUserRolesViewModel model)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.Updated = DateTime.UtcNow;
            user.UpdatedBy = HttpContext.User?.Identity?.Name;
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            result = await userManager.AddToRolesAsync(user, model.VendorUserRoleViewModel.
                Where(x => x.Selected).Select(y => y.RoleName));
            var currentUser = await userManager.GetUserAsync(HttpContext.User);
            await signInManager.RefreshSignInAsync(currentUser);
            var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user role edited. Email : " + user.Email);

            toastNotification.AddSuccessToastMessage("<i class='fas fa-user-cog'></i>  User role(s) updated successfully!");
            return RedirectToAction(nameof(AgencyController.User), "Agency");
        }

        [Breadcrumb("Manage Service")]
        public async Task<IActionResult> Service()
        {
            return View();
        }

        [Breadcrumb("Add Service", FromAction = "Service")]
        public IActionResult CreateService()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == vendorUser.VendorId);

            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name");
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            var model = new VendorInvestigationServiceType { SelectedMultiPincodeId = new List<string>(), Vendor = vendor, PincodeServices = new List<ServicedPinCode>() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(VendorInvestigationServiceType vendorInvestigationServiceType)
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == vendorUser.VendorId);

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
                vendorInvestigationServiceType.VendorId = vendor.VendorId;
                _context.Add(vendorInvestigationServiceType);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("<i class='fas fa-truck'></i> Service created successfully!");

                return RedirectToAction(nameof(AgencyController.Service), "Agency");
            }
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", vendorInvestigationServiceType.CountryId);
            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name", vendorInvestigationServiceType.LineOfBusinessId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", vendorInvestigationServiceType.DistrictId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", vendorInvestigationServiceType.StateId);
            toastNotification.AddErrorToastMessage("Error to create vendor service!");

            return View(vendorInvestigationServiceType);
        }

        [Breadcrumb("Edit Service", FromAction = "Service")]
        public async Task<IActionResult> EditService(string id)
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

            ViewBag.PinCodeId = _context.PinCode.Where(p => p.District.DistrictId == vendorInvestigationServiceType.DistrictId)
                .Select(x => new SelectListItem
                {
                    Text = x.Name + " - " + x.Code,
                    Value = x.PinCodeId.ToString()
                }).ToList();

            var selected = services.PincodeServices.Select(s => s.Pincode).ToList();
            services.SelectedMultiPincodeId = _context.PinCode.Where(p => selected.Contains(p.Code)).Select(p => p.PinCodeId).ToList();

            return View(services);
        }

        // POST: VendorService/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(string vendorInvestigationServiceTypeId, VendorInvestigationServiceType vendorInvestigationServiceType)
        {
            if (vendorInvestigationServiceTypeId != vendorInvestigationServiceType.VendorInvestigationServiceTypeId)
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
                        toastNotification.AddSuccessToastMessage("<i class='fas fa-truck'></i> Service updated successfully!");
                        return RedirectToAction(nameof(AgencyController.Service), "Agency");
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
                toastNotification.AddErrorToastMessage("Error: service edit!");
                return BadRequest();
            }
            ViewData["InvestigationServiceTypeId"] = new SelectList(_context.InvestigationServiceType, "InvestigationServiceTypeId", "Name", vendorInvestigationServiceType.InvestigationServiceTypeId);
            ViewData["LineOfBusinessId"] = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name", vendorInvestigationServiceType.LineOfBusinessId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", vendorInvestigationServiceType.StateId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", vendorInvestigationServiceType.DistrictId);
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "Name", vendorInvestigationServiceType.VendorId);
            return View(vendorInvestigationServiceType);
        }

        // GET: VendorService/Delete/5
        [Breadcrumb("Delete Service", FromAction = "Service")]
        public async Task<IActionResult> DeleteService(string id)
        {
            if (id == null || _context.VendorInvestigationServiceType == null)
            {
                return NotFound();
            }

            var vendorInvestigationServiceType = await _context.VendorInvestigationServiceType
                .Include(v => v.InvestigationServiceType)
                .Include(v => v.LineOfBusiness)
                .Include(v => v.PincodeServices)
                .Include(v => v.Country)
                .Include(v => v.State)
                .Include(v => v.District)
                .Include(v => v.Vendor)
                .FirstOrDefaultAsync(m => m.VendorInvestigationServiceTypeId == id);
            if (vendorInvestigationServiceType == null)
            {
                return NotFound();
            }

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
            return RedirectToAction("Service", "Agency");
        }

        [Breadcrumb("Services", FromAction = "Service")]
        public async Task<IActionResult> ServiceDetail(string id)
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

            return View(vendorInvestigationServiceType);
        }

        [Breadcrumb("Agent Load", FromAction = "User")]
        public async Task<IActionResult> AgentLoad()
        {
            return View();
        }

        private bool VendorInvestigationServiceTypeExists(string id)
        {
            return (_context.VendorInvestigationServiceType?.Any(e => e.VendorInvestigationServiceTypeId == id)).GetValueOrDefault();
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

        private void GetCountryStateEdit(VendorApplicationUser? user)
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", user?.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", user?.DistrictId);
            ViewData["StateId"] = new SelectList(_context.State.Where(s => s.CountryId == user.CountryId), "StateId", "Name", user?.StateId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode.Where(s => s.StateId == user.StateId), "PinCodeId", "Name", user?.PinCodeId);
        }

        private async Task<List<string>> GetUserRoles(VendorApplicationUser user)
        {
            return new List<string>(await userManager.GetRolesAsync(user));
        }

        private bool VendorExists(string id)
        {
            return (_context.Vendor?.Any(e => e.VendorId == id)).GetValueOrDefault();
        }
    }
}