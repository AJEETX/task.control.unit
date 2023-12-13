using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using NToastNotify;

using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Role")]
    public class UserRolesController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IToastNotification toastNotification;

        public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IToastNotification toastNotification,
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.toastNotification = toastNotification;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> Index(string userId)
        {
            var userRoles = new List<UserRoleViewModel>();
            //ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                toastNotification.AddErrorToastMessage("user not found!");
                return NotFound();
            }
            //ViewBag.UserName = user.UserName;
            foreach (var role in roleManager.Roles)
            {
                var userRoleViewModel = new UserRoleViewModel
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
            var model = new UserRolesViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                UserRoleViewModel = userRoles
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string userId, UserRolesViewModel model)
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
            result = await userManager.AddToRolesAsync(user, model.UserRoleViewModel.Where(x => x.Selected).Select(y => y.RoleName));
            var currentUser = await userManager.GetUserAsync(User);
            await signInManager.RefreshSignInAsync(currentUser);
            var response = SmsService.SendSingleMessage(user.PhoneNumber, "User role edited. Email : " + user.Email);

            toastNotification.AddSuccessToastMessage("roles updated successfully!");
            return RedirectToAction(nameof(UserController.Index), "User");
        }
    }
}