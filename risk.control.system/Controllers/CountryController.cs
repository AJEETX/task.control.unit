using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.Data;
using risk.control.system.Models;

using SmartBreadcrumbs.Attributes;

namespace risk.control.system.Controllers
{
    [Breadcrumb("Country")]
    public class CountryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification toastNotification;

        public CountryController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            this.toastNotification = toastNotification;
        }

        // GET: RiskCaseStatus
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Country.AsQueryable();

            var applicationDbContextResult = await applicationDbContext.ToListAsync();
            return View(applicationDbContextResult);

        }

        // GET: RiskCaseStatus/Details/5
        [Breadcrumb("Details")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }

            var country = await _context.Country
                .FirstOrDefaultAsync(m => m.CountryId == id);
            if (country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }

            return View(country);
        }

        [Breadcrumb("Add Country")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            country.Updated = DateTime.UtcNow;
            country.UpdatedBy = HttpContext.User?.Identity?.Name;
            _context.Add(country);
            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("country added successfully!");
            return RedirectToAction(nameof(Index));
        }

        // GET: RiskCaseStatus/Edit/5
        [Breadcrumb("Edit Country")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }

            var country = await _context.Country.FirstOrDefaultAsync(c => c.CountryId == id);
            if (country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }
            return View(country);
        }

        // POST: RiskCaseStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Country country)
        {
            if (id != country.CountryId)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    country.Updated = DateTime.UtcNow;
                    country.UpdatedBy = HttpContext.User?.Identity?.Name;
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.CountryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                toastNotification.AddSuccessToastMessage("country edited successfully!");
                return RedirectToAction(nameof(Index));
            }
            toastNotification.AddErrorToastMessage("Error to edit country!");
            return View(country);
        }

        // GET: RiskCaseStatus/Delete/5
        [Breadcrumb("Delete Country")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }

            var country = await _context.Country
                .FirstOrDefaultAsync(m => m.CountryId == id);
            if (country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return NotFound();
            }

            return View(country);
        }

        // POST: RiskCaseStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Country == null)
            {
                toastNotification.AddErrorToastMessage("country not found!");
                return Problem("Entity set 'ApplicationDbContext.Country'  is null.");
            }
            var country = await _context.Country.FindAsync(id);
            if (country != null)
            {
                country.Updated = DateTime.UtcNow;
                country.UpdatedBy = HttpContext.User?.Identity?.Name;
                _context.Country.Remove(country);
            }

            await _context.SaveChangesAsync();
            toastNotification.AddSuccessToastMessage("country deleted successfully!");
            return RedirectToAction(nameof(Index));
        }

        private bool CountryExists(string id)
        {
            return (_context.Country?.Any(e => e.CountryId == id)).GetValueOrDefault();
        }
    }
}