using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class VendorInvestigationServiceType : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string VendorInvestigationServiceTypeId { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Investigation service type")]
        public string InvestigationServiceTypeId { get; set; } = default!;

        [Display(Name = "Investigation service type")]
        public InvestigationServiceType InvestigationServiceType { get; set; } = default!;

        [Display(Name = "Line of business")]
        public string? LineOfBusinessId { get; set; } = default!;

        public LineOfBusiness? LineOfBusiness { get; set; } = default!;

        [Display(Name = "Country name")]
        public string? CountryId { get; set; } = default!;

        public Country? Country { get; set; } = default!;

        [Display(Name = "State")]
        public string? StateId { get; set; } = default!;

        public State? State { get; set; } = default!;

        [Display(Name = "District")]
        public string? DistrictId { get; set; } = default!;

        public District? District { get; set; } = default!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        [Display(Name = "Choose Multiple Pincodes")]
        public List<string> SelectedMultiPincodeId { get; set; } = new List<string> { }!;

        [Display(Name = "Serviced pincodes")]
        public List<ServicedPinCode> PincodeServices { get; set; } = default!;

        public string VendorId { get; set; }
        public Vendor Vendor { get; set; }
        public bool Deleted { get; set; } = false;
    }
}