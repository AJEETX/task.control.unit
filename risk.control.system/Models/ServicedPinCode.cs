using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class ServicedPinCode : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ServicedPinCodeId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = default!;
        public string Pincode { get; set; } = default!;
        [Display(Name = "Vendor services")]
        public string VendorInvestigationServiceTypeId { get; set; } = default!;
        public VendorInvestigationServiceType VendorInvestigationServiceType { get; set; } = default!;

    }
}
