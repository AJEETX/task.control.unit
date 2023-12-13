using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class ClientCompanyApplicationUser : ApplicationUser
    {
        [Display(Name = "Company")]
        public string? ClientCompanyId { get; set; } = default!;
        public ClientCompany? ClientCompany { get; set; } = default!;
        public string? Comments { get; set; } = default!;
    }
}
