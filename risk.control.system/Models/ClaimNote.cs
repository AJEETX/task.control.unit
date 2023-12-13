using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class ClaimNote : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ClaimNoteId { get; set; } = Guid.NewGuid().ToString();

        public string Sender { get; set; }
        public string Comment { get; set; }
        public ClaimNote? ParentClaimNote { get; set; }
    }
}