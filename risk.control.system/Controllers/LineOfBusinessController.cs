using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Line Of Business")]
    public class LineOfBusinessController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public LineOfBusinessController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: RiskCaseTypes
        public async Task<IActionResult> Index()
        {
            return _context.LineOfBusiness != null ?
                        View(await _context.LineOfBusiness.Include(l => l.InvestigationServiceTypes).ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.RiskCaseType'  is null.");
        }

        // GET: RiskCaseTypes/Details/5
        [Breadcrumb("Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.LineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }

            var lineOfBusiness = await _context.LineOfBusiness
                .FirstOrDefaultAsync(m => m.LineOfBusinessId == id);
            if (lineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }

            return View(lineOfBusiness);
        }

        // GET: RiskCaseTypes/Create
        [Breadcrumb("Add Line of Business")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RiskCaseTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LineOfBusiness lineOfBusiness)
        {
            lineOfBusiness.Updated = DateTime.UtcNow;
            lineOfBusiness.UpdatedBy = HttpContext.User?.Identity?.Name;

            _context.Add(lineOfBusiness);
            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("line of business created successfully!");
            return RedirectToAction(nameof(Index));
        }

        // GET: RiskCaseTypes/Edit/5
        [Breadcrumb("Edit Line of Business")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.LineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }

            var lineOfBusiness = await _context.LineOfBusiness.FindAsync(id);
            if (lineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }
            return View(lineOfBusiness);
        }

        // POST: RiskCaseTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, LineOfBusiness lineOfBusiness)
        {
            if (id != lineOfBusiness.LineOfBusinessId)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lineOfBusiness.Updated = DateTime.UtcNow;
                    lineOfBusiness.UpdatedBy = HttpContext.User?.Identity?.Name;

                    _context.Update(lineOfBusiness);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!lineOfBusinessTypeExists(lineOfBusiness.LineOfBusinessId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("line of business edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            toastNotification.AddErrorToastMessage("Error to edit line of business!");
            return View(lineOfBusiness);
        }

        // GET: RiskCaseTypes/Delete/5
        [Breadcrumb("Delete Line of Business")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.LineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }

            var lineOfBusiness = await _context.LineOfBusiness
                .FirstOrDefaultAsync(m => m.LineOfBusinessId == id);
            if (lineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return NotFound();
            }

            return View(lineOfBusiness);
        }

        // POST: RiskCaseTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.LineOfBusiness == null)
            {
                toastNotification.AddErrorToastMessage("line of business not found!");
                return Problem("Entity set 'ApplicationDbContext.RiskCaseType'  is null.");
            }
            var lineOfBusiness = await _context.LineOfBusiness.FindAsync(id);
            if (lineOfBusiness != null)
            {
                lineOfBusiness.Updated = DateTime.UtcNow;
                lineOfBusiness.UpdatedBy = HttpContext.User?.Identity?.Name;

                _context.LineOfBusiness.Remove(lineOfBusiness);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("line of business deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool lineOfBusinessTypeExists(string id)
        {
            return (_context.LineOfBusiness?.Any(e => e.LineOfBusinessId == id)).GetValueOrDefault();
        }
    }
}