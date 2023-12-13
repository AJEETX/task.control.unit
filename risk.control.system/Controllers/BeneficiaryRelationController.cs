using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Beneficairy Relation ")]
    public class BeneficiaryRelationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public BeneficiaryRelationController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: BeneficiaryRelation
        public async Task<IActionResult> Index(string SearchString)
        {
            return _context.BeneficiaryRelation != null ?
                        View(await _context.BeneficiaryRelation.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.BeneficiaryRelation'  is null.");
        }

        // GET: BeneficiaryRelation/Details/5
        [Breadcrumb("Details ")]
        public async Task<IActionResult> Details(long id)
        {
            if (id == null || _context.BeneficiaryRelation == null)
            {
                return NotFound();
            }

            var beneficiaryRelation = await _context.BeneficiaryRelation
                .FirstOrDefaultAsync(m => m.BeneficiaryRelationId == id);
            if (beneficiaryRelation == null)
            {
                return NotFound();
            }

            return View(beneficiaryRelation);
        }

        // GET: BeneficiaryRelation/Create
        [Breadcrumb("Add Beneficairy Relation ")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: BeneficiaryRelation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BeneficiaryRelation beneficiaryRelation)
        {
            if (beneficiaryRelation is not null)
            {
                beneficiaryRelation.Updated = DateTime.Now;
                beneficiaryRelation.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Add(beneficiaryRelation);
                await _context.SaveChangesAsync();
                toastNotification.AddSuccessToastMessage("beneficiary relation created successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(beneficiaryRelation);
        }

        // GET: BeneficiaryRelation/Edit/5
        [Breadcrumb("Edit Beneficairy Relation")]
        public async Task<IActionResult> Edit(long id)
        {
            if (id == null || _context.BeneficiaryRelation == null)
            {
                return NotFound();
            }

            var beneficiaryRelation = await _context.BeneficiaryRelation.FindAsync(id);
            if (beneficiaryRelation == null)
            {
                return NotFound();
            }
            return View(beneficiaryRelation);
        }

        // POST: BeneficiaryRelation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, BeneficiaryRelation beneficiaryRelation)
        {
            if (id != beneficiaryRelation.BeneficiaryRelationId)
            {
                return NotFound();
            }

            if (beneficiaryRelation is not null)
            {
                try
                {
                    beneficiaryRelation.Updated = DateTime.UtcNow;
                    beneficiaryRelation.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Update(beneficiaryRelation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BeneficiaryRelationExists(beneficiaryRelation.BeneficiaryRelationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("beneficiary relation edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            return View(beneficiaryRelation);
        }

        // GET: BeneficiaryRelation/Delete/5
        [Breadcrumb("Delete Beneficairy Relation")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id == null || _context.BeneficiaryRelation == null)
            {
                return NotFound();
            }

            var beneficiaryRelation = await _context.BeneficiaryRelation
                .FirstOrDefaultAsync(m => m.BeneficiaryRelationId == id);
            if (beneficiaryRelation == null)
            {
                return NotFound();
            }

            return View(beneficiaryRelation);
        }

        // POST: BeneficiaryRelation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.BeneficiaryRelation == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BeneficiaryRelation'  is null.");
            }
            var beneficiaryRelation = await _context.BeneficiaryRelation.FindAsync(id);
            if (beneficiaryRelation != null)
            {
                beneficiaryRelation.Updated = DateTime.UtcNow;
                beneficiaryRelation.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.BeneficiaryRelation.Remove(beneficiaryRelation);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("beneficiary relation deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool BeneficiaryRelationExists(long id)
        {
            return (_context.BeneficiaryRelation?.Any(e => e.BeneficiaryRelationId == id)).GetValueOrDefault();
        }
    }
}