using Microsoft.EntityFrameworkCore;

using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

namespace risk.control.system.Data
{
    public class ApplicationDbContext : AuditableIdentityContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor context) : base(options, context)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public virtual DbSet<UploadClaim> UploadClaim { get; set; }
        public virtual DbSet<ClaimNote> ClaimNote { get; set; }
        public virtual DbSet<ClaimMessage> ClaimMessage { get; set; }
        public virtual DbSet<ApplicationUser> ApplicationUser { get; set; }
        public virtual DbSet<InvestigationTransaction> InvestigationTransaction { get; set; }
        public virtual DbSet<ClientCompanyApplicationUser> ClientCompanyApplicationUser { get; set; }
        public virtual DbSet<ApplicationRole> ApplicationRole { get; set; }
        public virtual DbSet<InvestigationCase> InvestigationCase { get; set; }

        //public virtual DbSet<ClaimsInvestigation> ClaimsInvestigation { get; set; }
        public virtual DbSet<LineOfBusiness> LineOfBusiness { get; set; }

        public virtual DbSet<InvestigationCaseStatus> InvestigationCaseStatus { get; set; }
        public virtual DbSet<InvestigationCaseSubStatus> InvestigationCaseSubStatus { get; set; }
        public virtual DbSet<InvestigationCaseOutcome> InvestigationCaseOutcome { get; set; }
        public virtual DbSet<CostCentre> CostCentre { get; set; }
        public virtual DbSet<CaseEnabler> CaseEnabler { get; set; }
        public virtual DbSet<BeneficiaryRelation> BeneficiaryRelation { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<InboxMessage> InboxMessage { get; set; }
        public virtual DbSet<OutboxMessage> OutboxMessage { get; set; }
        public virtual DbSet<SentMessage> SentMessage { get; set; }
        public virtual DbSet<DraftMessage> DraftMessage { get; set; }
        public virtual DbSet<TrashMessage> TrashMessage { get; set; }
        public virtual DbSet<DeletedMessage> DeletedMessage { get; set; }
        public virtual DbSet<FileAttachment> FileAttachment { get; set; }
        public virtual DbSet<State> State { get; set; }
        public virtual DbSet<District> District { get; set; }
        public virtual DbSet<PinCode> PinCode { get; set; }
        public virtual DbSet<ClientCompany> ClientCompany { get; set; }
        public virtual DbSet<InvestigationServiceType> InvestigationServiceType { get; set; }
        public virtual DbSet<ServicedPinCode> ServicedPinCode { get; set; }
        public virtual DbSet<Vendor> Vendor { get; set; } = default!;
        public virtual DbSet<VendorInvestigationServiceType> VendorInvestigationServiceType { get; set; } = default!;
        public virtual DbSet<VendorApplicationUser> VendorApplicationUser { get; set; } = default!;
        public virtual DbSet<Mailbox> Mailbox { get; set; } = default!;
        public virtual DbSet<ClaimReport> ClaimReport { get; set; } = default!;

        public virtual DbSet<FileOnDatabaseModel> FilesOnDatabase { get; set; }
        public virtual DbSet<FileOnFileSystemModel> FilesOnFileSystem { get; set; }
        public DbSet<ClaimsInvestigation> ClaimsInvestigation { get; set; } = default!;
        public DbSet<VerificationLocation> VerificationLocation { get; set; } = default!;
        public DbSet<CaseLocation> CaseLocation { get; set; } = default!;
        public DbSet<VerifyPinCode> VerifyPinCode { get; set; } = default!;
        public DbSet<PolicyDetail> PolicyDetail { get; set; } = default!;
        public DbSet<CustomerDetail> CustomerDetail { get; set; } = default!;
    }
}