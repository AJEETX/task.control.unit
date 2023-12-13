using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Case Status")]
    public class InvestigationCaseStatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public InvestigationCaseStatusController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: RiskCaseStatus
        public async Task<IActionResult> Index()
        {
            return _context.InvestigationCaseStatus != null ?
                        View(await _context.InvestigationCaseStatus.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.RiskCaseStatus'  is null.");
        }

        // GET: RiskCaseStatus/Details/5
        [Breadcrumb("Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.InvestigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }

            var investigationCaseStatus = await _context.InvestigationCaseStatus
                .FirstOrDefaultAsync(m => m.InvestigationCaseStatusId == id);
            if (investigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }

            return View(investigationCaseStatus);
        }

        // GET: RiskCaseStatus/Create
        [Breadcrumb("Add Case Status")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RiskCaseStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestigationCaseStatus investigationCaseStatus)
        {
            investigationCaseStatus.Updated = DateTime.UtcNow;
            investigationCaseStatus.UpdatedBy = HttpContext.User?.Identity?.Name;
            _context.Add(investigationCaseStatus);
            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("case status created successfully!");
            return RedirectToAction(nameof(Index));
        }

        // GET: RiskCaseStatus/Edit/5
        [Breadcrumb("Edit Case Status")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.InvestigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }

            var investigationCaseStatus = await _context.InvestigationCaseStatus.FindAsync(id);
            if (investigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }
            return View(investigationCaseStatus);
        }

        // POST: RiskCaseStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, InvestigationCaseStatus investigationCaseStatus)
        {
            if (id != investigationCaseStatus.InvestigationCaseStatusId)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }

            if (investigationCaseStatus is not null)
            {
                try
                {
                    investigationCaseStatus.Updated = DateTime.UtcNow;
                    investigationCaseStatus.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Update(investigationCaseStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvestigationCaseStatusExists(investigationCaseStatus.InvestigationCaseStatusId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("case status edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            toastNotification.AddErrorToastMessage("Error to edit status!");
            return View(investigationCaseStatus);
        }

        // GET: RiskCaseStatus/Delete/5
        [Breadcrumb("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.InvestigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }

            var investigationCaseStatus = await _context.InvestigationCaseStatus
                .FirstOrDefaultAsync(m => m.InvestigationCaseStatusId == id);
            if (investigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return NotFound();
            }

            return View(investigationCaseStatus);
        }

        // POST: RiskCaseStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.InvestigationCaseStatus == null)
            {
                toastNotification.AddErrorToastMessage("status not found!");
                return Problem("Entity set 'ApplicationDbContext.RiskCaseStatus'  is null.");
            }
            var investigationCaseStatus = await _context.InvestigationCaseStatus.FindAsync(id);
            if (investigationCaseStatus != null)
            {
                investigationCaseStatus.Updated = DateTime.UtcNow;
                investigationCaseStatus.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.InvestigationCaseStatus.Remove(investigationCaseStatus);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("case status deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool InvestigationCaseStatusExists(string id)
        {
            return (_context.InvestigationCaseStatus?.Any(e => e.InvestigationCaseStatusId == id)).GetValueOrDefault();
        }
    }
}
