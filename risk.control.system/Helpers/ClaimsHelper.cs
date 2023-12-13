using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Security.Claims;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.AppConstant;

namespace risk.control.system.Helpers
{
    public static class ClaimsHelper
    {
        public static void GetPermissions(this List<RoleClaimsViewModel> allPermissions, Type policy)
        {
            FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo fi in fields)
            {
                allPermissions.Add(new RoleClaimsViewModel { Value = fi.GetValue(null).ToString(), Type = Applicationsettings.PERMISSION });
            }
        }

        public static async Task AddPermissionClaim(this RoleManager<ApplicationRole> roleManager, ApplicationRole role, string permission)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            if (!allClaims.Any(a => a.Type == Applicationsettings.PERMISSION && a.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim(Applicationsettings.PERMISSION, permission));
            }
        }
    }
}
