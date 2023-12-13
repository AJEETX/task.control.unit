using risk.control.system.AppConstant;

namespace risk.control.system.Helpers
{
    public static class Permissions
    {
        public static List<string> GeneratePermissionsForModule(string module)
        {
            return new List<string>()
            {
                ModuleManager.GetModule(module, Applicationsettings.CREATE),
                ModuleManager.GetModule(module, Applicationsettings.VIEW),
                ModuleManager.GetModule(module, Applicationsettings.EDIT),
                ModuleManager.GetModule(module, Applicationsettings.DELETE),
            };
        }

        public static class Underwriting
        {
            public static string View = ModuleManager.GetModule(nameof(Underwriting), Applicationsettings.VIEW);
            public static string Create = ModuleManager.GetModule(nameof(Underwriting), Applicationsettings.CREATE);
            public static string Edit = ModuleManager.GetModule(nameof(Underwriting), Applicationsettings.EDIT);
            public static string Delete = ModuleManager.GetModule(nameof(Underwriting), Applicationsettings.DELETE);
        }
        public static class Claim
        {
            public static string View = ModuleManager.GetModule(nameof(Claim), Applicationsettings.VIEW);
            public static string Create = ModuleManager.GetModule(nameof(Claim), Applicationsettings.CREATE);
            public static string Edit = ModuleManager.GetModule(nameof(Claim), Applicationsettings.EDIT);
            public static string Delete = ModuleManager.GetModule(nameof(Claim), Applicationsettings.DELETE);
        }
        public static class ModuleManager
        {
            public static string GetModule(string module, string action)
            {
                return $"{Applicationsettings.PERMISSION}.{module}.{action}";
            }
        }
    }
}
