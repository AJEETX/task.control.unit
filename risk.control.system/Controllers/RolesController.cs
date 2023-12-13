using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using risk.control.system.Models;
using NToastNotify;
using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Roles")]
    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IToastNotification toastNotification;

        public RolesController(RoleManager<ApplicationRole> roleManager, IToastNotification toastNotification)
        {
            _roleManager = roleManager;
            this.toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index(string id = null)
        {
            var roles = await _roleManager.Roles.ToListAsync();

            return View(roles);
        }

        [Breadcrumb("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole role)
        {
            if (role != null)
            {
                await _roleManager.CreateAsync(role);
            }
            toastNotification.AddSuccessToastMessage("role created successfully!");
            return RedirectToAction(nameof(Index));
        }

        [Breadcrumb("Edit")]
        public async Task<IActionResult> Edit(string Id)
        {
            ApplicationRole role = null;
            if (!string.IsNullOrEmpty(Id))
            {
                role = await _roleManager.FindByIdAsync(Id);
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id, ApplicationRole role)
        {
            if (role is not null)
            {
                var existingRole = await _roleManager.FindByIdAsync(role.Id.ToString());
                existingRole.Name = role.Name;
                await _roleManager.UpdateAsync(existingRole);
                toastNotification.AddSuccessToastMessage("role edited successfully!");
                return RedirectToAction(nameof(Index));
            }

            toastNotification.AddErrorToastMessage("Error to edit role!");
            return View(role);
        }

        [Breadcrumb("Delete")]
        public async Task<IActionResult> Delete(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);

            if (role == null)
            {
                toastNotification.AddErrorToastMessage("role not found!");
                return NotFound();
            }

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
            toastNotification.AddSuccessToastMessage("role deleted successfully!");
            return RedirectToAction(nameof(Index));
        }
    }
}