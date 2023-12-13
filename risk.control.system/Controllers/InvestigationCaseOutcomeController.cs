using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Case Outcome")]
    public class InvestigationCaseOutcomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public InvestigationCaseOutcomeController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: InvestigationCaseOutcome
        public async Task<IActionResult> Index()
        {
            return _context.InvestigationCaseOutcome != null ?
                        View(await _context.InvestigationCaseOutcome.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.InvestigationCaseOutcome'  is null.");
        }

        // GET: InvestigationCaseOutcome/Details/5
        [Breadcrumb("Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.InvestigationCaseOutcome == null)
            {
                return NotFound();
            }

            var investigationCaseOutcome = await _context.InvestigationCaseOutcome
                .FirstOrDefaultAsync(m => m.InvestigationCaseOutcomeId == id);
            if (investigationCaseOutcome == null)
            {
                return NotFound();
            }

            return View(investigationCaseOutcome);
        }

        // GET: InvestigationCaseOutcome/Create
        [Breadcrumb("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: InvestigationCaseOutcome/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvestigationCaseOutcome investigationCaseOutcome)
        {
            if (investigationCaseOutcome is not null)
            {
                investigationCaseOutcome.Updated = DateTime.UtcNow;
                investigationCaseOutcome.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Add(investigationCaseOutcome);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("case outcome created successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(investigationCaseOutcome);
        }

        // GET: InvestigationCaseOutcome/Edit/5
        [Breadcrumb("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.InvestigationCaseOutcome == null)
            {
                return NotFound();
            }

            var investigationCaseOutcome = await _context.InvestigationCaseOutcome.FindAsync(id);
            if (investigationCaseOutcome == null)
            {
                return NotFound();
            }
            return View(investigationCaseOutcome);
        }

        // POST: InvestigationCaseOutcome/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, InvestigationCaseOutcome investigationCaseOutcome)
        {
            if (id != investigationCaseOutcome.InvestigationCaseOutcomeId)
            {
                return NotFound();
            }

            if (investigationCaseOutcome is not null)
            {
                try
                {
                    investigationCaseOutcome.Updated = DateTime.UtcNow;
                    investigationCaseOutcome.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Update(investigationCaseOutcome);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvestigationCaseOutcomeExists(investigationCaseOutcome.InvestigationCaseOutcomeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("case outcome edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(investigationCaseOutcome);
        }

        // GET: InvestigationCaseOutcome/Delete/5
        [Breadcrumb("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.InvestigationCaseOutcome == null)
            {
                return NotFound();
            }

            var investigationCaseOutcome = await _context.InvestigationCaseOutcome
                .FirstOrDefaultAsync(m => m.InvestigationCaseOutcomeId == id);
            if (investigationCaseOutcome == null)
            {
                return NotFound();
            }

            return View(investigationCaseOutcome);
        }

        // POST: InvestigationCaseOutcome/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.InvestigationCaseOutcome == null)
            {
                return Problem("Entity set 'ApplicationDbContext.InvestigationCaseOutcome'  is null.");
            }
            var investigationCaseOutcome = await _context.InvestigationCaseOutcome.FindAsync(id);
            if (investigationCaseOutcome != null)
            {
                investigationCaseOutcome.Updated = DateTime.UtcNow;
                investigationCaseOutcome.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.InvestigationCaseOutcome.Remove(investigationCaseOutcome);
            }

            toastNotification.AddSuccessToastMessage("case outcome deleted successfully!");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvestigationCaseOutcomeExists(string id)
        {
            return (_context.InvestigationCaseOutcome?.Any(e => e.InvestigationCaseOutcomeId == id)).GetValueOrDefault();
        }
    }
}
