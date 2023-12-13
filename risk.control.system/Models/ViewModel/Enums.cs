using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models.ViewModel
{
    public enum Domain
    {
        [Display(Name = ".com")]
        com,

        [Display(Name = ".co.in")]
        co_in,

        [Display(Name = ".in")]
        _in_
    }
}