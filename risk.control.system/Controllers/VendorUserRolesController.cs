using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

namespace risk.control.system.Controllers
{
    [Breadcrumb(" Agency")]
    public class VendorUserRolesController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IToastNotification toastNotification;

        public VendorUserRolesController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IToastNotification toastNotification,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.toastNotification = toastNotification;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> Index(string userId)
        {
            var userRoles = new List<VendorUserRoleViewModel>();
            //ViewBag.userId = userId;
            VendorApplicationUser user = (VendorApplicationUser)await userManager.FindByIdAsync(userId);
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
            var editPage = new MvcBreadcrumbNode("Index", "VendorUser", $"Manage Users") { Parent = agencyPage, RouteValues = new { id = user.VendorId } };
            var userRolePage = new MvcBreadcrumbNode("Index", "VendorUserRoles", $"Edit Role") { Parent = editPage, RouteValues = new { userid = userId } };
            ViewData["BreadcrumbNode"] = userRolePage;

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
            result = await userManager.AddToRolesAsync(user, model.VendorUserRoleViewModel.Where(x => x.Selected).Select(y => y.RoleName));
            var currentUser = await userManager.GetUserAsync(User);
            await signInManager.RefreshSignInAsync(currentUser);

            var response = SmsService.SendSingleMessage(user.PhoneNumber, "User role edited. Email : " + user.Email);
            toastNotification.AddSuccessToastMessage("roles updated successfully!");
            return RedirectToAction(nameof(VendorUserController.Index), "VendorUser", new { Id = model.VendorId });
        }
    }
}