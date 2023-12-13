namespace risk.control.system.Models.ViewModel
{

    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<UserRoleViewModel> UserRoleViewModel { get; set; }

    }
    public class UserRoleViewModel
    {

        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }

    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public IList<UserRoleViewModel> UserRoles { get; set; }
    }
}
