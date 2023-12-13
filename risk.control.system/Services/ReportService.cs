using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Helpers;

namespace risk.control.system.Services
{
    public interface IReportService
    {
        Task<string> PrintPdf(string id);
    }

    public class ReportService : IReportService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ApplicationDbContext context;

        public ReportService(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.context = context;
        }

        public async Task<string> PrintPdf(string id)
        {
            var claim = context.ClaimsInvestigation
                .Include(c => c.PolicyDetail)
                .Include(c => c.CustomerDetail)
                .Include(c => c.CaseLocations)
                .ThenInclude(r => r.ClaimReport)
                .FirstOrDefault(c => c.ClaimsInvestigationId == id);

            var policy = claim.PolicyDetail;
            var customer = claim.CustomerDetail;
            var beneficiary = claim.CaseLocations.FirstOrDefault();
            var report = claim.CaseLocations.FirstOrDefault()?.ClaimReport;

            var file = "report.pdf";

            string folder = Path.Combine(webHostEnvironment.WebRootPath, Path.GetFileNameWithoutExtension(file));

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = Path.Combine(webHostEnvironment.WebRootPath, Path.GetFileNameWithoutExtension(file), file);

            ReportRunner.Run(webHostEnvironment.WebRootPath).Build(filePath); ;
            if (file == null) return null;
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return file;
        }
    }
}