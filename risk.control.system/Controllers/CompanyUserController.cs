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
    [Breadcrumb("Company User", FromController = typeof(ClientCompanyController))]
    public class CompanyUserController : Controller
    {
        public List<UsersViewModel> UserList;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;
        private readonly IPasswordHasher<ClientCompanyApplicationUser> passwordHasher;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IToastNotification toastNotification;
        private readonly ApplicationDbContext _context;

        public CompanyUserController(UserManager<ClientCompanyApplicationUser> userManager,
            IPasswordHasher<ClientCompanyApplicationUser> passwordHasher,
            RoleManager<ApplicationRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IToastNotification toastNotification,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.passwordHasher = passwordHasher;
            this.roleManager = roleManager;
            this.webHostEnvironment = webHostEnvironment;
            this.toastNotification = toastNotification;
            this._context = context;
            UserList = new List<UsersViewModel>();
        }

        public async Task<IActionResult> Index(string id, string sortOrder, string currentFilter, string searchString, int? currentPage, int pageSize = 10)
        {
            ViewBag.EmailSortParm = string.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PincodeSortParm = string.IsNullOrEmpty(sortOrder) ? "pincode_desc" : "";
            if (searchString != null)
            {
                currentPage = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            var company = _context.ClientCompany
                .Include(c => c.CompanyApplicationUser)
                .FirstOrDefault(c => c.ClientCompanyId == id);

            var applicationDbContext = company.CompanyApplicationUser
                .AsQueryable();

            applicationDbContext = applicationDbContext
                .Include(c => c.Country)
                .Include(c => c.State)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .Include(c => c.ClientCompany)
                .Where(u => u.ClientCompanyId == id);

            var model = new CompanyUsersViewModel
            {
                Company = company,
            };

            //if (applicationDbContext.Any())
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    applicationDbContext = applicationDbContext.Where(a =>
                    a.FirstName.ToLower().Contains(searchString.Trim().ToLower()) ||
                    a.LastName.ToLower().Contains(searchString.Trim().ToLower()));
                }

                switch (sortOrder)
                {
                    case "name_desc":
                        applicationDbContext = applicationDbContext.OrderByDescending(s => new { s.FirstName, s.LastName });
                        break;

                    case "email_desc":
                        applicationDbContext = applicationDbContext.OrderByDescending(s => s.Email);
                        break;

                    case "pincode_desc":
                        applicationDbContext = applicationDbContext.OrderByDescending(s => s.PinCode.Code);
                        break;

                    default:
                        applicationDbContext.OrderByDescending(s => s.Email);
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

                var users = applicationDbContext.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                var tempusers = userManager.Users.Where(c => c.ClientCompanyId == id);
                foreach (var user in users)
                {
                    var country = _context.Country.FirstOrDefault(c => c.CountryId == user.CountryId);
                    var state = _context.State.FirstOrDefault(c => c.StateId == user.StateId);
                    var district = _context.District.FirstOrDefault(c => c.DistrictId == user.DistrictId);
                    var pinCode = _context.PinCode.FirstOrDefault(c => c.PinCodeId == user.PinCodeId);

                    var thisViewModel = new UsersViewModel();
                    thisViewModel.UserId = user.Id.ToString();
                    thisViewModel.Email = user?.Email;
                    thisViewModel.UserName = user?.UserName;
                    thisViewModel.ProfileImage = user?.ProfilePictureUrl ?? Applicationsettings.NO_IMAGE;
                    thisViewModel.FirstName = user.FirstName;
                    thisViewModel.LastName = user.LastName;
                    thisViewModel.Addressline = user.Addressline;
                    thisViewModel.PhoneNumber = user.PhoneNumber;
                    thisViewModel.Country = country.Name;
                    thisViewModel.CountryId = user.CountryId;
                    thisViewModel.StateId = user.StateId;
                    thisViewModel.State = state.Name;
                    thisViewModel.PinCode = pinCode.Name + "-" + pinCode.Code;
                    thisViewModel.PinCodeId = pinCode.PinCodeId;
                    thisViewModel.CompanyName = company.Name;
                    thisViewModel.CompanyId = user.ClientCompanyId;
                    thisViewModel.ProfileImageInByte = user.ProfilePicture;
                    thisViewModel.Roles = await GetUserRoles(user);
                    UserList.Add(thisViewModel);
                }
                model.Users = UserList;
            }
            return View(model);
        }

        [Breadcrumb("Details")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.ClientCompanyApplicationUser == null)
            {
                return NotFound();
            }

            var clientApplicationUser = await _context.ClientCompanyApplicationUser
                .Include(v => v.Country)
                .Include(v => v.District)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .Include(v => v.ClientCompany)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clientApplicationUser == null)
            {
                return NotFound();
            }

            return View(clientApplicationUser);
        }

        // GET: ClientCompanyApplicationUser/Create
        [Breadcrumb("Add User", FromController = typeof(ClientCompanyController))]
        public IActionResult Create(string id)
        {
            var company = _context.ClientCompany.FirstOrDefault(v => v.ClientCompanyId == id);
            var model = new ClientCompanyApplicationUser { ClientCompany = company };
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            return View(model);
        }

        // POST: ClientCompanyApplicationUser/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCompanyApplicationUser user, string emailSuffix)
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
                var response = SmsService.SendSingleMessage(user.PhoneNumber, "Company account created. Domain : " + user.Email);

                return RedirectToAction(nameof(CompanyUserController.Index), "CompanyUser", new { id = user.ClientCompanyId });
            }
            else
            {
                toastNotification.AddErrorToastMessage("Error to create user!");
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            GetCountryStateEdit(user);
            toastNotification.AddSuccessToastMessage("user created successfully!");
            return View(user);
        }

        private void GetCountryStateEdit(ClientCompanyApplicationUser? user)
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", user?.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", user?.DistrictId);
            ViewData["StateId"] = new SelectList(_context.State.Where(s => s.CountryId == user.CountryId), "StateId", "Name", user?.StateId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode.Where(s => s.StateId == user.StateId), "PinCodeId", "Name", user?.PinCodeId);
        }

        // GET: ClientCompanyApplicationUser/Edit/5
        [Breadcrumb("Edit User", FromController = typeof(ClientCompanyController))]
        public async Task<IActionResult> Edit(long? userId)
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
            return View(clientCompanyApplicationUser);
        }

        // POST: ClientCompanyApplicationUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ClientCompanyApplicationUser applicationUser)
        {
            if (id != applicationUser.Id.ToString())
            {
                toastNotification.AddErrorToastMessage("company not found!");
                return NotFound();
            }

            if (applicationUser is not null)
            {
                try
                {
                    var user = await userManager.FindByIdAsync(id);
                    if (applicationUser?.ProfileImage != null && applicationUser.ProfileImage.Length > 0)
                    {
                        string newFileName = user.Email + Guid.NewGuid().ToString();
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
                        user.ProfilePicture = applicationUser?.ProfilePicture ?? user.ProfilePicture;
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
                            toastNotification.AddSuccessToastMessage("Company user edited successfully!");
                            var response = SmsService.SendSingleMessage(user.PhoneNumber, "Company account edited. Domain : " + user.Email);

                            return RedirectToAction(nameof(CompanyUserController.Index), "CompanyUser", new { id = applicationUser.ClientCompanyId });
                        }
                        toastNotification.AddErrorToastMessage("Error !!. The user con't be edited!");
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

            toastNotification.AddErrorToastMessage("Error to create Company user!");
            return RedirectToAction(nameof(CompanyUserController.Index), "CompanyUser", new { id = applicationUser.ClientCompany });
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        // GET: VendorApplicationUsers/Delete/5
        [Breadcrumb("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.VendorApplicationUser == null)
            {
                return NotFound();
            }

            var vendorApplicationUser = await _context.VendorApplicationUser
                .Include(v => v.Country)
                .Include(v => v.District)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .Include(v => v.Vendor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vendorApplicationUser == null)
            {
                return NotFound();
            }

            return View(vendorApplicationUser);
        }

        // POST: VendorApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.ClientCompanyApplicationUser == null)
            {
                return Problem("Entity set 'ApplicationDbContext.VendorApplicationUser'  is null.");
            }
            var clientCompanyApplicationUser = await _context.ClientCompanyApplicationUser.FindAsync(id);
            if (clientCompanyApplicationUser != null)
            {
                clientCompanyApplicationUser.Updated = DateTime.UtcNow;
                clientCompanyApplicationUser.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.ClientCompanyApplicationUser.Remove(clientCompanyApplicationUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CompanyUserController.Index), "CompanyUser", new { id = clientCompanyApplicationUser.ClientCompanyId });
        }

        private bool VendorApplicationUserExists(long id)
        {
            return (_context.VendorApplicationUser?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task<List<string>> GetUserRoles(ClientCompanyApplicationUser user)
        {
            return new List<string>(await userManager.GetRolesAsync(user));
        }
    }
}