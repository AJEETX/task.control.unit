namespace risk.control.system.Models.ViewModel
{

    public class CompanyUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CompanyId { get; set; }
        public string Company { get; set; }
        public List<CompanyUserRoleViewModel> CompanyUserRoleViewModel { get; set; }

    }
    public class CompanyUserRoleViewModel
    {

        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }

    public class ManageCompanyUserRolesViewModel
    {
        public string CompanyId { get; set; }
        public IList<CompanyUserRoleViewModel> CompanyUserRoles { get; set; }
    }
}
