using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace risk.control.system.Models
{
    public class Mailbox :BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MailboxId { get; set; }
        public string Name { get; set; } = default!;
        public List<InboxMessage> Inbox { get; set; } = new();
        public List<OutboxMessage> Outbox { get; set; } = new();
        public List<SentMessage> Sent { get; set; } = new();
        public List<DraftMessage> Draft { get; set; } = new();
        public List<TrashMessage> Trash { get; set; } = new();
        public List<DeletedMessage> Deleted { get; set; } = new();
        public long ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
