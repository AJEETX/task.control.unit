namespace risk.control.system.Models.ViewModel
{
    public class PermissionsViewModel
    {
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public List<PermissionViewModel> PermissionViewModels { get; set; }
    }

    public class PermissionViewModel
    {
        public List<RoleClaimsViewModel> RoleClaims { get; set; } = new List<RoleClaimsViewModel>();
    }

    public class RoleClaimsViewModel
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
