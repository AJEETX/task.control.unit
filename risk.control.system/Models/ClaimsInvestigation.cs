using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class ClaimsInvestigation : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ClaimsInvestigationId { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Agency name")]
        public string? VendorId { get; set; }

        [Display(Name = "Agency name")]
        public Vendor? Vendor { get; set; }

        public string? PolicyDetailId { get; set; }
        public PolicyDetail? PolicyDetail { get; set; }
        public string? CustomerDetailId { get; set; }
        public CustomerDetail? CustomerDetail { get; set; }

        [NotMapped]
        public bool HasClientCompany { get; set; } = true;

        public List<Vendor>? Vendors { get; set; } = new();

        [Display(Name = "Case status")]
        public string? InvestigationCaseStatusId { get; set; } = default!;

        [Display(Name = "Case status")]
        public InvestigationCaseStatus? InvestigationCaseStatus { get; set; } = default!;

        [Display(Name = "Case sub status")]
        public string? InvestigationCaseSubStatusId { get; set; } = default!;

        [Display(Name = "Case sub status")]
        public InvestigationCaseSubStatus? InvestigationCaseSubStatus { get; set; } = default!;

        [NotMapped]
        public bool SelectedToAssign { get; set; }

        public string? CurrentUserEmail { get; set; }
        public List<CaseLocation>? CaseLocations { get; set; }
        public List<ClaimNote>? ClaimNotes { get; set; } = new();
        public string? CurrentClaimOwner { get; set; }
        public List<ClaimMessage>? ClaimMessages { get; set; } = new();

        public override string ToString()
        {
            return $"Case Id: {ClaimsInvestigationId}, <br /> ";
        }

        public bool IsReviewCase { get; set; } = false;
        public bool IsReady2Assign { get; set; } = false;
        public bool Deleted { get; set; } = false;
    }

    public enum Income
    {
        [Display(Name = "UNKNOWN")]
        UNKNOWN,

        [Display(Name = "0.0 Lac")]
        NO_INCOME,

        [Display(Name = "0 - 2.5 Lac")]
        TAXFREE_SLOT,

        [Display(Name = "2.5 - 5 Lac")]
        BASIC_INCOME,

        [Display(Name = "5 - 8 Lac")]
        MEDIUUM_INCOME,

        [Display(Name = "8 - 15 Lac")]
        UPPER_INCOME,

        [Display(Name = "15 - 30 Lac")]
        HIGHER_INCOME,

        [Display(Name = "30 - 50 Lac")]
        TOP_HIGHER_INCOME,

        [Display(Name = "50 + Lac")]
        PREMIUM_INCOME,
    }

    public enum Occupation
    {
        UNKNOWN,

        [Display(Name = "UNEMPLOYED")]
        UNEMPLOYED,

        [Display(Name = "DOCTOR")]
        DOCTOR,

        [Display(Name = "ENGINEER")]
        ENGINEER,

        [Display(Name = "ACCOUNTANT")]
        ACCOUNTANT,

        [Display(Name = "SELF EMPLOYED")]
        SELF_EMPLOYED,

        [Display(Name = "OTHER")]
        OTHER
    }

    public enum Education
    {
        [Display(Name = "PRIMARY SCHOOL")]
        PRIMARY_SCHOOL,

        [Display(Name = "HIGH SCHOOL")]
        HIGH_SCHOOL,

        [Display(Name = "12th Class")]
        CLASS_12,

        [Display(Name = "GRADUATE")]
        GRADUATE,

        [Display(Name = "POST GRADUATE")]
        POST_GRADUATE,

        [Display(Name = "PROFESSIONAL")]
        PROFESSIONAL,
    }

    public class VerificationLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string VerificationLocationId { get; set; } = Guid.NewGuid().ToString();

        public virtual ClaimsInvestigation ClaimsInvestigation { get; set; } = default!;

        [Display(Name = "Address line")]
        public string? Addressline { get; set; }

        [Display(Name = "PinCode name")]
        public string? PinCodeId { get; set; } = default!;

        [Display(Name = "PinCode name")]
        public PinCode? PinCode { get; set; } = default!;

        [Display(Name = "State name")]
        public string? StateId { get; set; } = default!;

        [Display(Name = "State name")]
        public State? State { get; set; } = default!;

        [Required]
        [Display(Name = "Country name")]
        public string CountryId { get; set; } = default!;

        [Display(Name = "Country name")]
        public Country Country { get; set; } = default!;

        [Display(Name = "District")]
        public string? DistrictId { get; set; } = default!;

        [Display(Name = "District")]
        public District? District { get; set; } = default!;
    }

    public enum ClaimType
    {
        [Display(Name = "Death")]
        DEATH,

        [Display(Name = "Health")]
        HEALTH
    }

    public enum Gender
    {
        [Display(Name = "Male")]
        MALE,

        [Display(Name = "Female")]
        FEMALE,

        [Display(Name = "Other")]
        OTHER
    }

    public enum CustomerType
    {
        [Display(Name = "HNI")]
        HNI,

        [Display(Name = "Non-HNI")]
        NONHNI,
    }
}