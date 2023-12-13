using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
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
using SmartBreadcrumbs.Nodes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Agencies")]
    public class VendorApplicationUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<VendorApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IToastNotification toastNotification;

        public VendorApplicationUsersController(ApplicationDbContext context,
            UserManager<VendorApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IToastNotification toastNotification)
        {
            _context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.webHostEnvironment = webHostEnvironment;
            this.toastNotification = toastNotification;
        }

        // GET: VendorApplicationUsers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.VendorApplicationUser.Include(v => v.Country).Include(v => v.District).Include(v => v.PinCode).Include(v => v.State).Include(v => v.Vendor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: VendorApplicationUsers/Details/5

        public async Task<IActionResult> Details(long? id)
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
            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var usersPage = new MvcBreadcrumbNode("Index", "VendorApplicationUsers", $"Manage Users") { Parent = agencyPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Details", "VendorApplicationUsers", $"User Details") { Parent = usersPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = editPage;

            return View(vendorApplicationUser);
        }

        // GET: VendorApplicationUsers/Create

        public IActionResult Create(string id)
        {
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == id);
            var model = new VendorApplicationUser { Vendor = vendor };
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var usersPage = new MvcBreadcrumbNode("Index", "VendorUser", $"Manage Users") { Parent = agencyPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Create", "VendorApplicationUsers", $"Add User") { Parent = usersPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = editPage;

            return View(model);
        }

        // POST: VendorApplicationUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorApplicationUser user, string emailSuffix)
        {
            if (user.ProfileImage != null && user.ProfileImage.Length > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                string fileExtension = Path.GetExtension(user.ProfileImage.FileName);
                newFileName += fileExtension;
                var upload = Path.Combine(webHostEnvironment.WebRootPath, "img", newFileName);
                user.ProfileImage.CopyTo(new FileStream(upload, FileMode.Create));
                user.ProfilePictureUrl = "/img/" + newFileName;
            }
            var userFullEmail = user.Email.Trim().ToLower() + "@" + emailSuffix;
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
                        var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user created and locked. Email : " + user.Email);
                        toastNotification.AddSuccessToastMessage("<i class='fas fa-user-lock'></i> User created and locked successfully!");
                    }
                }
                else
                {
                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user created. Email : " + user.Email);
                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-plus'></i> User created successfully!");
                }
                return RedirectToAction(nameof(VendorUserController.Index), "VendorUser", new { id = user.VendorId });
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

        private void GetCountryStateEdit(VendorApplicationUser? user)
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", user?.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", user?.DistrictId);
            ViewData["StateId"] = new SelectList(_context.State.Where(s => s.CountryId == user.CountryId), "StateId", "Name", user?.StateId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode.Where(s => s.StateId == user.StateId), "PinCodeId", "Name", user?.PinCodeId);
        }

        // GET: VendorApplicationUsers/Edit/5

        public async Task<IActionResult> Edit(long? userId)
        {
            if (userId == null || _context.VendorApplicationUser == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return NotFound();
            }

            var vendorApplicationUser = _context.VendorApplicationUser
                .Include(v => v.Mailbox).Where(v => v.Id == userId)
                ?.FirstOrDefault();
            if (vendorApplicationUser == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return NotFound();
            }
            var vendor = _context.Vendor.FirstOrDefault(v => v.VendorId == vendorApplicationUser.VendorId);

            if (vendor == null)
            {
                toastNotification.AddErrorToastMessage("vendor not found");
                return NotFound();
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

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = vendor.VendorId } };
            var usersPage = new MvcBreadcrumbNode("Index", "VendorUser", $"Manage Users") { Parent = agencyPage, RouteValues = new { id = vendor.VendorId } };
            var editPage = new MvcBreadcrumbNode("Edit", "VendorApplicationUsers", $"Edit User") { Parent = usersPage, RouteValues = new { id = userId } };
            ViewData["BreadcrumbNode"] = editPage;

            return View(vendorApplicationUser);
        }

        // POST: VendorApplicationUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VendorApplicationUser applicationUser)
        {
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
                                var lockDate = await userManager.SetLockoutEndDateAsync(createdUser, DateTime.MaxValue);

                                if (lockUser.Succeeded && lockDate.Succeeded)
                                {
                                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user edited and locked. Email : " + user.Email);
                                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-lock'></i> User edited and locked successfully!");
                                }
                            }
                            else
                            {
                                var createdUser = await userManager.FindByEmailAsync(user.Email);
                                var lockUser = await userManager.SetLockoutEnabledAsync(createdUser, true);
                                var lockDate = await userManager.SetLockoutEndDateAsync(createdUser, DateTime.Now);

                                if (lockUser.Succeeded && lockDate.Succeeded)
                                {
                                    var response = SmsService.SendSingleMessage(user.PhoneNumber, "Agency user edited and unlocked. Email : " + user.Email);
                                    toastNotification.AddSuccessToastMessage("<i class='fas fa-user-check'></i> User edited and unlocked successfully!");
                                }
                            }
                            return RedirectToAction(nameof(VendorUserController.Index), "VendorUser", new { id = applicationUser.VendorId });
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

            toastNotification.AddErrorToastMessage("Error to create agency user!");
            return RedirectToAction(nameof(VendorUserController.Index), "VendorUser", new { id = applicationUser.VendorId });
        }

        // GET: VendorApplicationUsers/Delete/5
        public async Task<IActionResult> UserRoles(string userId)
        {
            var userRoles = new List<VendorUserRoleViewModel>();
            //ViewBag.userId = userId;
            VendorApplicationUser user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return NotFound();
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
            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = user.VendorId } };
            var usersPage = new MvcBreadcrumbNode("Index", "VendorUser", $"Manage Users") { Parent = agencyPage, RouteValues = new { id = user.VendorId } };
            var editPage = new MvcBreadcrumbNode("UserRoles", "VendorApplicationUsers", $"Edit Role") { Parent = usersPage, RouteValues = new { id = user.Id } };
            ViewData["BreadcrumbNode"] = editPage;
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
            //var currentUser = await userManager.GetUserAsync(HttpContext.User);
            //await signInManager.RefreshSignInAsync(currentUser);

            toastNotification.AddSuccessToastMessage("<i class='fas fa-user-cog'></i>  User role(s) updated successfully!");
            return RedirectToAction(nameof(VendorUserController.Index), "VendorUser", new { id = model.VendorId });
        }

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

            var agencysPage = new MvcBreadcrumbNode("Index", "Vendors", "All Agencies");
            var agencyPage = new MvcBreadcrumbNode("Details", "Vendors", "Manage Agency") { Parent = agencysPage, RouteValues = new { id = id } };
            var usersPage = new MvcBreadcrumbNode("Index", "VendorApplicationUsers", $"Manage Users") { Parent = agencyPage, RouteValues = new { id = id } };
            var editPage = new MvcBreadcrumbNode("Delete", "VendorApplicationUsers", $"Delete User") { Parent = usersPage, RouteValues = new { id = id } };
            ViewData["BreadcrumbNode"] = editPage;

            return View(vendorApplicationUser);
        }

        // POST: VendorApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.VendorApplicationUser == null)
            {
                return Problem("Entity set 'ApplicationDbContext.VendorApplicationUser'  is null.");
            }
            var vendorApplicationUser = await _context.VendorApplicationUser.FindAsync(id);
            if (vendorApplicationUser != null)
            {
                vendorApplicationUser.Updated = DateTime.UtcNow;
                vendorApplicationUser.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.VendorApplicationUser.Remove(vendorApplicationUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        private bool VendorApplicationUserExists(long id)
        {
            return (_context.VendorApplicationUser?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}