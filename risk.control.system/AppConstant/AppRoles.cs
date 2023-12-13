using System.ComponentModel.DataAnnotations;

namespace risk.control.system.AppConstant
{
    public enum AppRoles
    {
        [Display(Name = Applicationsettings.PORTAL_ADMIN.DISPLAY_NAME)] PortalAdmin,
        [Display(Name = Applicationsettings.ADMIN.DISPLAY_NAME)] CompanyAdmin,
        [Display(Name = Applicationsettings.AGENCY_ADMIN.DISPLAY_NAME)] AgencyAdmin,
        [Display(Name = Applicationsettings.CREATOR.DISPLAY_NAME)] Creator,
        [Display(Name = Applicationsettings.ASSIGNER.DISPLAY_NAME)] Assigner,
        [Display(Name = Applicationsettings.ASSESSOR.DISPLAY_NAME)] Assessor,
        [Display(Name = Applicationsettings.SUPERVISOR.DISPLAY_NAME)] Supervisor,
        [Display(Name = Applicationsettings.AGENT.DISPLAY_NAME)] Agent
    }
}