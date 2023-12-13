using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class FileAttachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FileAttachmentId { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        [NotMapped]
        public IFormFile? Attachment { get; set; }
        public byte[]? AttachedDocument { get; set; }

        public string? ContactMessageId { get; set; }
        //public MailboxMessage? ContactMessage { get; set; }

        public string? ClaimsInvestigationId { get; set; }
        public ClaimsInvestigation? ClaimsInvestigation { get; set; }
    }
}
