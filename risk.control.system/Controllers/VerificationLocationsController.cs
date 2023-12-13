using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using risk.control.system.Data;
using risk.control.system.Models;

namespace risk.control.system.Controllers
{
    public class VerificationLocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VerificationLocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VerificationLocations
        public async Task<IActionResult> Index(string id)
        {
            var applicationDbContext = _context.VerificationLocation
                .Include(v => v.ClaimsInvestigation)
                .Include(v => v.Country)
                .Include(v => v.District)
                .Include(v => v.PinCode)
                .Include(v => v.State).Where(l=>l.ClaimsInvestigation.ClaimsInvestigationId == id);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: VerificationLocations/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.VerificationLocation == null)
            {
                return NotFound();
            }

            var verificationLocation = await _context.VerificationLocation
                .Include(v => v.Country)
                .Include(v => v.District)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .FirstOrDefaultAsync(m => m.VerificationLocationId == id);
            if (verificationLocation == null)
            {
                return NotFound();
            }

            return View(verificationLocation);
        }

        // GET: VerificationLocations/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name");
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name");
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "Name");
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name");
            return View();
        }

        // POST: VerificationLocations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VerificationLocationId,Addressline,PinCodeId,StateId,CountryId,DistrictId")] VerificationLocation verificationLocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(verificationLocation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", verificationLocation.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", verificationLocation.DistrictId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "Name", verificationLocation.PinCodeId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", verificationLocation.StateId);
            return View(verificationLocation);
        }

        // GET: VerificationLocations/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.VerificationLocation == null)
            {
                return NotFound();
            }

            var verificationLocation = await _context.VerificationLocation.FindAsync(id);
            if (verificationLocation == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", verificationLocation.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", verificationLocation.DistrictId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "Name", verificationLocation.PinCodeId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", verificationLocation.StateId);
            return View(verificationLocation);
        }

        // POST: VerificationLocations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VerificationLocation verificationLocation)
        {
            if (id != verificationLocation.VerificationLocationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(verificationLocation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VerificationLocationExists(verificationLocation.VerificationLocationId))
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
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "Name", verificationLocation.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "Name", verificationLocation.DistrictId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "Name", verificationLocation.PinCodeId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "Name", verificationLocation.StateId);
            return View(verificationLocation);
        }

        // GET: VerificationLocations/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.VerificationLocation == null)
            {
                return NotFound();
            }

            var verificationLocation = await _context.VerificationLocation
                .Include(v => v.Country)
                .Include(v => v.District)
                .Include(v => v.PinCode)
                .Include(v => v.State)
                .FirstOrDefaultAsync(m => m.VerificationLocationId == id);
            if (verificationLocation == null)
            {
                return NotFound();
            }

            return View(verificationLocation);
        }

        // POST: VerificationLocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.VerificationLocation == null)
            {
                return Problem("Entity set 'ApplicationDbContext.VerificationLocation'  is null.");
            }
            var verificationLocation = await _context.VerificationLocation.FindAsync(id);
            if (verificationLocation != null)
            {
                _context.VerificationLocation.Remove(verificationLocation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VerificationLocationExists(string id)
        {
          return (_context.VerificationLocation?.Any(e => e.VerificationLocationId == id)).GetValueOrDefault();
        }
    }
}
