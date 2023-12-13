using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Case SubStatus")]
    public class InvestigationCaseSubStatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public InvestigationCaseSubStatusController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: InvestigationCaseSubStatus
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.InvestigationCaseSubStatus
                .Include(i => i.InvestigationCaseStatus);
            return View(await applicationDbContext.ToListAsync());
        }

        [HttpPost, ActionName("GetSubstatusBystatusId")]
        public async Task<IActionResult> GetSubstatusBystatusId(string InvestigationCaseStatusId)
        {
            string lId;
            var subStatuses = new List<InvestigationCaseSubStatus>();
            if (!string.IsNullOrEmpty(InvestigationCaseStatusId))
            {
                lId = InvestigationCaseStatusId;
                subStatuses = await _context.InvestigationCaseSubStatus
                    .Include(i => i.InvestigationCaseStatus).Where(s =>
                    s.InvestigationCaseStatus.InvestigationCaseStatusId.Equals(lId)).ToListAsync();
            }
            return Ok(subStatuses?.Select(s => new { s.Code, s.InvestigationCaseSubStatusId }));
        }

        // GET: InvestigationCaseSubStatus/Details/5
        [Breadcrumb("Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.InvestigationCaseSubStatus == null)
            {
                return NotFound();
            }

            var investigationCaseSubStatus = await _context.InvestigationCaseSubStatus
                .Include(i => i.InvestigationCaseStatus)
                .FirstOrDefaultAsync(m => m.InvestigationCaseSubStatusId == id);
            if (investigationCaseSubStatus == null)
            {
                return NotFound();
            }

            return View(investigationCaseSubStatus);
        }

        // GET: InvestigationCaseSubStatus/Create
        [Breadcrumb("Add Case SubStatus")]
        public IActionResult Create()
        {
            ViewData["InvestigationCaseStatusId"] = new SelectList(_context.InvestigationCaseStatus, "InvestigationCaseStatusId", "Name");
            return View();
        }

        // POST: InvestigationCaseSubStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestigationCaseSubStatus investigationCaseSubStatus)
        {
            if (investigationCaseSubStatus is not null)
            {
                investigationCaseSubStatus.Updated = DateTime.UtcNow;
                investigationCaseSubStatus.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Add(investigationCaseSubStatus);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("case sub-status created successfully!");
                return RedirectToAction(nameof(Index));
            }
            toastNotification.AddErrorToastMessage("case sub-status create failed!!");
            ViewData["InvestigationCaseStatusId"] = new SelectList(_context.InvestigationCaseStatus, "InvestigationCaseStatusId", "Name", investigationCaseSubStatus.InvestigationCaseStatusId);
            return View(investigationCaseSubStatus);
        }

        // GET: InvestigationCaseSubStatus/Edit/5
        [Breadcrumb("Edit Case SubStatus")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.InvestigationCaseSubStatus == null)
            {
                return NotFound();
            }

            var investigationCaseSubStatus = await _context.InvestigationCaseSubStatus.FindAsync(id);
            if (investigationCaseSubStatus == null)
            {
                return NotFound();
            }
            ViewData["InvestigationCaseStatusId"] = new SelectList(_context.InvestigationCaseStatus, "InvestigationCaseStatusId", "Name", investigationCaseSubStatus.InvestigationCaseStatusId);
            return View(investigationCaseSubStatus);
        }

        // POST: InvestigationCaseSubStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, InvestigationCaseSubStatus investigationCaseSubStatus)
        {
            if (id != investigationCaseSubStatus.InvestigationCaseSubStatusId)
            {
                return NotFound();
            }

            if (investigationCaseSubStatus is not null)
            {
                try
                {
                    investigationCaseSubStatus.Updated = DateTime.UtcNow;
                    investigationCaseSubStatus.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Update(investigationCaseSubStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvestigationCaseSubStatusExists(investigationCaseSubStatus.InvestigationCaseSubStatusId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("case sub-status edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            ViewData["InvestigationCaseStatusId"] = new SelectList(_context.InvestigationCaseStatus, "InvestigationCaseStatusId", "Name", investigationCaseSubStatus.InvestigationCaseStatusId);
            return View(investigationCaseSubStatus);
        }

        // GET: InvestigationCaseSubStatus/Delete/5
        [Breadcrumb("Delete Case SubStatus")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.InvestigationCaseSubStatus == null)
            {
                return NotFound();
            }

            var investigationCaseSubStatus = await _context.InvestigationCaseSubStatus
                .Include(i => i.InvestigationCaseStatus)
                .FirstOrDefaultAsync(m => m.InvestigationCaseSubStatusId == id);
            if (investigationCaseSubStatus == null)
            {
                return NotFound();
            }

            return View(investigationCaseSubStatus);
        }

        // POST: InvestigationCaseSubStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.InvestigationCaseSubStatus == null)
            {
                return Problem("Entity set 'ApplicationDbContext.InvestigationCaseSubStatus'  is null.");
            }
            var investigationCaseSubStatus = await _context.InvestigationCaseSubStatus.FindAsync(id);
            if (investigationCaseSubStatus != null)
            {
                investigationCaseSubStatus.Updated = DateTime.UtcNow;
                investigationCaseSubStatus.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.InvestigationCaseSubStatus.Remove(investigationCaseSubStatus);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("case sub-status deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool InvestigationCaseSubStatusExists(string id)
        {
            return (_context.InvestigationCaseSubStatus?.Any(e => e.InvestigationCaseSubStatusId == id)).GetValueOrDefault();
        }
    }
}