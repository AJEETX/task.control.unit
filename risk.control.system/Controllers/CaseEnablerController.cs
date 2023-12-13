using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Reason To Verify ")]
    public class CaseEnablerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public CaseEnablerController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: CaseEnabler
        public async Task<IActionResult> Index(string searchString)
        {
            return _context.CaseEnabler != null ?
                        View(await _context.CaseEnabler.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.CaseEnabler'  is null.");
        }

        // GET: CaseEnabler/Details/5
        [Breadcrumb("Details ")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.CaseEnabler == null)
            {
                return NotFound();
            }

            var caseEnabler = await _context.CaseEnabler
                .FirstOrDefaultAsync(m => m.CaseEnablerId == id);
            if (caseEnabler == null)
            {
                return NotFound();
            }

            return View(caseEnabler);
        }

        // GET: CaseEnabler/Create
        [Breadcrumb("Add Reason To Verify ")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CaseEnabler/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseEnabler caseEnabler)
        {
            if (caseEnabler is not null)
            {
                caseEnabler.Updated = DateTime.UtcNow;
                caseEnabler.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Add(caseEnabler);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("case enabler created successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(caseEnabler);
        }

        // GET: CaseEnabler/Edit/5
        [Breadcrumb("Edit Reason To Verify ")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.CaseEnabler == null)
            {
                return NotFound();
            }

            var caseEnabler = await _context.CaseEnabler.FindAsync(id);
            if (caseEnabler == null)
            {
                return NotFound();
            }
            return View(caseEnabler);
        }

        // POST: CaseEnabler/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CaseEnabler caseEnabler)
        {
            if (id != caseEnabler.CaseEnablerId)
            {
                return NotFound();
            }

            if (caseEnabler is not null)
            {
                try
                {
                    caseEnabler.Updated = DateTime.UtcNow;
                    caseEnabler.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Update(caseEnabler);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaseEnablerExists(caseEnabler.CaseEnablerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("case enabler edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(caseEnabler);
        }

        // GET: CaseEnabler/Delete/5
        [Breadcrumb("Delete Reason To Verify ")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.CaseEnabler == null)
            {
                return NotFound();
            }

            var caseEnabler = await _context.CaseEnabler
                .FirstOrDefaultAsync(m => m.CaseEnablerId == id);
            if (caseEnabler == null)
            {
                return NotFound();
            }

            return View(caseEnabler);
        }

        // POST: CaseEnabler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.CaseEnabler == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CaseEnabler'  is null.");
            }
            var caseEnabler = await _context.CaseEnabler.FindAsync(id);
            if (caseEnabler != null)
            {
                caseEnabler.Updated = DateTime.UtcNow;
                caseEnabler.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.CaseEnabler.Remove(caseEnabler);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("case enabler deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool CaseEnablerExists(string id)
        {
            return (_context.CaseEnabler?.Any(e => e.CaseEnablerId == id)).GetValueOrDefault();
        }
    }
}