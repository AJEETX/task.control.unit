//using System.Data;

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//using NToastNotify;

//using risk.control.system.Data;
//using risk.control.system.Models;

//using SmartBreadcrumbs.Attributes;

//namespace risk.control.system.Controllers
//{
//    [Breadcrumb("Case")]
//    public class InvestigationCaseController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IToastNotification toastNotification;
//        private readonly IWebHostEnvironment webHostEnvironment;

//        public InvestigationCaseController(ApplicationDbContext context, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment)
//        {
//            _context = context;
//            this.toastNotification = toastNotification;
//            this.webHostEnvironment = webHostEnvironment;
//        }

//        // GET: RiskCases
//        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? currentPage, int pageSize = 10)
//        {
//            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
//            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
//            if (searchString != null)
//            {
//                currentPage = 1;
//            }
//            else
//            {
//                searchString = currentFilter;
//            }

//            ViewBag.CurrentFilter = searchString;
//            var cases = _context.InvestigationCase.Include(r => r.InvestigationCaseStatus).Include(r => r.LineOfBusiness).AsQueryable();
//            if (!String.IsNullOrEmpty(searchString))
//            {
//                cases = cases.Where(s =>
//                 s.Name.ToLower().Contains(searchString.Trim().ToLower()) ||
//                 s.InvestigationCaseStatus.Name.ToLower().Contains(searchString.Trim().ToLower()) ||
//                 s.InvestigationCaseStatus.Code.ToLower().Contains(searchString.Trim().ToLower()) ||
//                 s.LineOfBusiness.Name.ToLower().Contains(searchString.Trim().ToLower()) ||
//                 s.LineOfBusiness.Code.ToLower().Contains(searchString.Trim().ToLower()) ||
//                 s.Description.ToLower().Contains(searchString.Trim().ToLower())
//                 );
//            }
//            switch (sortOrder)
//            {
//                case "name_desc":
//                    cases = cases.OrderByDescending(s => s.Name);
//                    break;

//                case "Date":
//                    cases = cases.OrderBy(s => s.Created);
//                    break;

//                case "date_desc":
//                    cases = cases.OrderByDescending(s => s.Created);
//                    break;

//                default:
//                    cases = cases.OrderBy(s => s.Created);
//                    break;
//            }

//            int pageNumber = (currentPage ?? 1);
//            ViewBag.TotalPages = (int)Math.Ceiling(decimal.Divide(cases.Count(), pageSize));
//            ViewBag.PageNumber = pageNumber;
//            ViewBag.PageSize = pageSize;
//            ViewBag.ShowPrevious = pageNumber > 1;
//            ViewBag.ShowNext = pageNumber < (int)Math.Ceiling(decimal.Divide(cases.Count(), pageSize));
//            ViewBag.ShowFirst = pageNumber != 1;
//            ViewBag.ShowLast = pageNumber != (int)Math.Ceiling(decimal.Divide(cases.Count(), pageSize));

//            var caseResult = await cases.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
//            return View(caseResult);
//        }

//        // GET: RiskCases/Details/5
//        public async Task<IActionResult> Details(string id)
//        {
//            if (id == null || _context.InvestigationCase == null)
//            {
//                return NotFound();
//            }

//            var investigationCase = await _context.InvestigationCase
//                .Include(r => r.InvestigationCaseStatus)
//                .Include(r => r.LineOfBusiness)
//                .FirstOrDefaultAsync(m => m.InvestigationId == id);
//            if (investigationCase == null)
//            {
//                return NotFound();
//            }

//            return View(investigationCase);
//        }

//        // GET: RiskCases/Create
//        [Breadcrumb("Add Case")]
//        public IActionResult Create()
//        {
//            ViewBag.InvestigationCaseStatusId = new SelectList(_context.InvestigationCaseStatus, "InvestigationCaseStatusId", "Name");
//            ViewBag.LineOfBusinessId = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name");
//            return View();
//        }

//        // POST: RiskCases/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(InvestigationCase investigationCase)
//        {
//            _context.Add(investigationCase);
//            await _context.SaveChangesAsync();
//            toastNotification.AddSuccessToastMessage("case created successfully!");
//            return RedirectToAction(nameof(Index));
//        }

//        // GET: RiskCases/Edit/5
//        [Breadcrumb("Edit Case")]
//        public async Task<IActionResult> Edit(string id)
//        {
//            if (id == null || _context.InvestigationCase == null)
//            {
//                toastNotification.AddErrorToastMessage("case not found!");
//                return NotFound();
//            }

//            var investigationCase = await _context.InvestigationCase.FindAsync(id);
//            if (investigationCase == null)
//            {
//                toastNotification.AddErrorToastMessage("case not found!");
//                return NotFound();
//            }
//            ViewBag.InvestigationCaseStatusId = new SelectList(_context.InvestigationCaseStatus, "InvestigationCaseStatusId", "Name", investigationCase.InvestigationCaseStatusId);
//            ViewBag.LineOfBusinessId = new SelectList(_context.LineOfBusiness, "LineOfBusinessId", "Name", investigationCase.LineOfBusinessId);
//            return View(investigationCase);
//        }

//        // POST: RiskCases/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(string id, InvestigationCase investigationCase)
//        {
//            if (id != investigationCase.InvestigationId)
//            {
//                toastNotification.AddErrorToastMessage("case not found!");
//                return NotFound();
//            }

//            try
//            {
//                _context.Update(investigationCase);
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!InvestigationCaseExists(investigationCase.InvestigationId))
//                {
//                    return NotFound();
//                }
//                else
//                {
//                    throw;
//                }
//            }
//            toastNotification.AddSuccessToastMessage("district edited successfully!");
//            return RedirectToAction(nameof(Index));
//        }

//        // GET: RiskCases/Delete/5
//        public async Task<IActionResult> Delete(string id)
//        {
//            if (id == null || _context.InvestigationCase == null)
//            {
//                toastNotification.AddErrorToastMessage("case not found!");
//                return NotFound();
//            }

//            var investigationCase = await _context.InvestigationCase
//                .Include(r => r.InvestigationCaseStatus)
//                .Include(r => r.LineOfBusiness)
//                .FirstOrDefaultAsync(m => m.InvestigationId == id);
//            if (investigationCase == null)
//            {
//                toastNotification.AddErrorToastMessage("case not found!");
//                return NotFound();
//            }

//            return View(investigationCase);
//        }

//        // POST: RiskCases/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(string id)
//        {
//            if (_context.InvestigationCase == null)
//            {
//                toastNotification.AddErrorToastMessage("case not found!");
//                return Problem("Entity set 'ApplicationDbContext.RiskCase'  is null.");
//            }
//            var investigationCase = await _context.InvestigationCase.FindAsync(id);
//            if (investigationCase != null)
//            {
//                _context.InvestigationCase.Remove(investigationCase);
//            }

//            await _context.SaveChangesAsync();
//            toastNotification.AddSuccessToastMessage("district deleted successfully!");
//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost]
//        public async Task<IActionResult> Upload(IFormFile postedFile)
//        {
//            if (postedFile != null)
//            {
//                string path = Path.Combine(webHostEnvironment.WebRootPath, "upload-cases");
//                if (!Directory.Exists(path))
//                {
//                    Directory.CreateDirectory(path);
//                }

//                string fileName = Path.GetFileName(postedFile.FileName);
//                string filePath = Path.Combine(path, fileName);
//                using (FileStream stream = new FileStream(filePath, FileMode.Create))
//                {
//                    postedFile.CopyTo(stream);
//                }
//                string csvData = await System.IO.File.ReadAllTextAsync(filePath);
//                DataTable dt = new DataTable();
//                bool firstRow = true;
//                foreach (string row in csvData.Split('\n'))
//                {
//                    if (!string.IsNullOrEmpty(row))
//                    {
//                        if (!string.IsNullOrEmpty(row))
//                        {
//                            if (firstRow)
//                            {
//                                foreach (string cell in row.Split(','))
//                                {
//                                    dt.Columns.Add(cell.Trim());
//                                }
//                                firstRow = false;
//                            }
//                            else
//                            {
//                                dt.Rows.Add();
//                                int i = 0;
//                                foreach (string cell in row.Split(','))
//                                {
//                                    dt.Rows[dt.Rows.Count - 1][i] = cell.Trim();
//                                    i++;
//                                }
//                            }
//                        }
//                    }
//                }

//                toastNotification.AddSuccessToastMessage("upload successfull!");
//                return View(new { DataTable = dt });
//            }
//            toastNotification.AddErrorToastMessage("Error in upload!");
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Broadcast(string[] caseIds)
//        {
//            await Task.Delay(1);
//            return Ok();
//        }

//        private bool InvestigationCaseExists(string id)
//        {
//            return (_context.InvestigationCase?.Any(e => e.InvestigationId == id)).GetValueOrDefault();
//        }
//    }
//}