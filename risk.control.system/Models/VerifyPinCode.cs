using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class VerifyPinCode : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string VerifyPinCodeId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = default!;
        public string Pincode { get; set; } = default!;
        [Display(Name = "verify services")]
        public long CaseLocationId { get; set; } = default!;
        public CaseLocation CaseLocation { get; set; } = default!;

    }
}
