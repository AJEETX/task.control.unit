using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models
{
    public class ClaimReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ClaimReportId { get; set; } = Guid.NewGuid().ToString();

        public string? VendorId { get; set; }
        public Vendor? Vendor { get; set; }
        public string? AgentEmail { get; set; }

        public DateTime? AgentRemarksUpdated { get; set; }
        public string? AgentRemarks { get; set; }
        public string? Question1 { get; set; }
        public string? Question2 { get; set; }
        public string? Question3 { get; set; }
        public string? Question4 { get; set; }
        public string? Question5 { get; set; }

        [Display(Name = "Agent Location Image")]
        public string? AgentLocationPictureUrl { get; set; }

        [Display(Name = "Agent Location Image")]
        public byte[]? AgentLocationPicture { get; set; }

        [Display(Name = "Location Data")]
        public string? LocationData { get; set; }

        public string? LocationPictureConfidence { get; set; } = string.Empty;

        [Display(Name = "Agent Location Image")]
        [NotMapped]
        public IFormFile? AgentLocationImage { get; set; }

        [Display(Name = "Agent Ocr Image")]
        public string? AgentOcrUrl { get; set; }

        [Display(Name = "Agent Ocr Image")]
        public byte[]? AgentOcrPicture { get; set; }

        [Display(Name = "Agent Ocr Image")]
        [NotMapped]
        public IFormFile? AgentOcrImage { get; set; }

        public bool? PanValid { get; set; } = false;
        public string? ImageType { get; set; }

        [Display(Name = "Agent Ocr Data")]
        public string? AgentOcrData { get; set; }

        [Display(Name = "Agent Qr Image")]
        public string? AgentQrUrl { get; set; }

        [Display(Name = "Agent Qr Image")]
        public byte[]? AgentQrPicture { get; set; }

        [Display(Name = "Agent Qr Image")]
        [NotMapped]
        public IFormFile? AgentQrImage { get; set; }

        [Display(Name = "Agent Qr Data")]
        public string? QrData { get; set; }

        public string? LocationLongLat { get; set; }
        public DateTime? LocationLongLatTime { get; set; } = DateTime.UtcNow;
        public string? OcrLongLat { get; set; }
        public DateTime? OcrLongLatTime { get; set; } = DateTime.UtcNow;
        public string? AgentReportId { get; set; }

        public AgentReport? AgentReport { get; set; }

        [Display(Name = "Supervisor Document")]
        public byte[]? SupervisorPicture { get; set; }

        [Display(Name = "Supervisor Document")]
        [NotMapped]
        public IFormFile? SupervisorDocumentImage { get; set; }

        public DateTime? SupervisorRemarksUpdated { get; set; }
        public string? SupervisorEmail { get; set; }
        public string? SupervisorRemarks { get; set; }
        public SupervisorRemarkType? SupervisorRemarkType { get; set; }
        public DateTime? AssessorRemarksUpdated { get; set; }
        public string? AssessorEmail { get; set; }
        public string? AssessorRemarks { get; set; }
        public AssessorRemarkType? AssessorRemarkType { get; set; }

        public long CaseLocationId { get; set; }
        public CaseLocation CaseLocation { get; set; }
    }

    public enum SupervisorRemarkType
    {
        OK,
        REVIEW
    }

    public enum AssessorRemarkType
    {
        OK,
        REVIEW,
    }

    public enum OcrImageType
    {
        PAN,
        ADHAAR,
        DL
    }
}