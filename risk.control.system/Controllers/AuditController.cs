using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Models;

namespace risk.control.system.Controllers
{
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Audit
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? currentPage, int pageSize = 10)
        {

            if (searchString != null)
            {
                currentPage = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var audits = _context.AuditLogs.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                audits = audits.Where(s =>
                 s.TableName.ToLower().Contains(searchString.Trim().ToLower()) ||
                 s.Type.ToLower().Contains(searchString.Trim().ToLower()) ||
                 s.OldValues.ToLower().Contains(searchString.Trim().ToLower()) ||
                 s.NewValues.ToLower().Contains(searchString.Trim().ToLower())
                 );
            }

            int pageNumber = (currentPage ?? 1);
            ViewBag.TotalPages = (int)Math.Ceiling(decimal.Divide(audits.Count(), pageSize));
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.ShowPrevious = pageNumber > 1;
            ViewBag.ShowNext = pageNumber < (int)Math.Ceiling(decimal.Divide(audits.Count(), pageSize));
            ViewBag.ShowFirst = pageNumber != 1;
            ViewBag.ShowLast = pageNumber != (int)Math.Ceiling(decimal.Divide(audits.Count(), pageSize));

            var auditsResult = await audits.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return _context.AuditLogs != null ?
                        View(auditsResult) :
                        Problem("Entity set 'ApplicationDbContext.AuditLogs'  is null.");
        }

        // GET: Audit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AuditLogs == null)
            {
                return NotFound();
            }

            var audit = await _context.AuditLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (audit == null)
            {
                return NotFound();
            }

            return View(audit);
        }

        // GET: Audit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Audit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Type,TableName,DateTime,OldValues,NewValues,AffectedColumns,PrimaryKey")] Audit audit)
        {
            if (ModelState.IsValid)
            {
                _context.Add(audit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(audit);
        }

        // GET: Audit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AuditLogs == null)
            {
                return NotFound();
            }

            var audit = await _context.AuditLogs.FindAsync(id);
            if (audit == null)
            {
                return NotFound();
            }
            return View(audit);
        }

        // POST: Audit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserId,Type,TableName,DateTime,OldValues,NewValues,AffectedColumns,PrimaryKey")] Audit audit)
        {
            if (id != audit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(audit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditExists(audit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(audit);
        }

        // GET: Audit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AuditLogs == null)
            {
                return NotFound();
            }

            var audit = await _context.AuditLogs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (audit == null)
            {
                return NotFound();
            }

            return View(audit);
        }

        // POST: Audit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AuditLogs == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AuditLogs'  is null.");
            }
            var audit = await _context.AuditLogs.FindAsync(id);
            if (audit != null)
            {
                _context.AuditLogs.Remove(audit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditExists(long id)
        {
            return (_context.AuditLogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
