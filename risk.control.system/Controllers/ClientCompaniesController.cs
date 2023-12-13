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
    public class ClientCompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientCompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ClientCompanies
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClientCompany.Include(c => c.Country).Include(c => c.District).Include(c => c.PinCode).Include(c => c.State);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClientCompanies/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ClientCompany == null)
            {
                return NotFound();
            }

            var clientCompany = await _context.ClientCompany
                .Include(c => c.Country)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.ClientCompanyId == id);
            if (clientCompany == null)
            {
                return NotFound();
            }

            return View(clientCompany);
        }

        // GET: ClientCompanies/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "CountryId");
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "DistrictId");
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "PinCodeId");
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "StateId");
            return View();
        }

        // POST: ClientCompanies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCompany clientCompany)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clientCompany);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "CountryId", clientCompany.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "DistrictId", clientCompany.DistrictId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "PinCodeId", clientCompany.PinCodeId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "StateId", clientCompany.StateId);
            return View(clientCompany);
        }

        // GET: ClientCompanies/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ClientCompany == null)
            {
                return NotFound();
            }

            var clientCompany = await _context.ClientCompany.FindAsync(id);
            if (clientCompany == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "CountryId", clientCompany.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "DistrictId", clientCompany.DistrictId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "PinCodeId", clientCompany.PinCodeId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "StateId", clientCompany.StateId);
            return View(clientCompany);
        }

        // POST: ClientCompanies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ClientCompanyId,Name,Code,Description,PhoneNumber,Email,Branch,Addressline,StateId,CountryId,PinCodeId,DistrictId,BankName,BankAccountNumber,IFSCCode,AgreementDate,ActivatedDate,Status,DocumentUrl,DocumentImage,Created,Updated,UpdatedBy")] ClientCompany clientCompany)
        {
            if (id != clientCompany.ClientCompanyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clientCompany);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientCompanyExists(clientCompany.ClientCompanyId))
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
            ViewData["CountryId"] = new SelectList(_context.Country, "CountryId", "CountryId", clientCompany.CountryId);
            ViewData["DistrictId"] = new SelectList(_context.District, "DistrictId", "DistrictId", clientCompany.DistrictId);
            ViewData["PinCodeId"] = new SelectList(_context.PinCode, "PinCodeId", "PinCodeId", clientCompany.PinCodeId);
            ViewData["StateId"] = new SelectList(_context.State, "StateId", "StateId", clientCompany.StateId);
            return View(clientCompany);
        }

        // GET: ClientCompanies/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ClientCompany == null)
            {
                return NotFound();
            }

            var clientCompany = await _context.ClientCompany
                .Include(c => c.Country)
                .Include(c => c.District)
                .Include(c => c.PinCode)
                .Include(c => c.State)
                .FirstOrDefaultAsync(m => m.ClientCompanyId == id);
            if (clientCompany == null)
            {
                return NotFound();
            }

            return View(clientCompany);
        }

        // POST: ClientCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ClientCompany == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ClientCompany'  is null.");
            }
            var clientCompany = await _context.ClientCompany.FindAsync(id);
            if (clientCompany != null)
            {
                _context.ClientCompany.Remove(clientCompany);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientCompanyExists(string id)
        {
          return (_context.ClientCompany?.Any(e => e.ClientCompanyId == id)).GetValueOrDefault();
        }
    }
}
