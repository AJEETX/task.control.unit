using CsvHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

using NToastNotify;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

using SmartBreadcrumbs.Attributes;
using SmartBreadcrumbs.Nodes;

using System.Data;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using risk.control.system.Helpers;

namespace risk.control.system.Controllers
{
    public class UploadsController : Controller
    {
        private static string NO_DATA = " NO - DATA ";
        private static Regex regex = new Regex("\\\"(.*?)\\\"");
        private readonly ApplicationDbContext _context;
        private readonly IFtpService ftpService;
        private readonly IHttpClientService httpClientService;
        private readonly IClaimsInvestigationService claimsInvestigationService;
        private readonly IMailboxService mailboxService;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IToastNotification toastNotification;
        private static HttpClient httpClient = new();

        public UploadsController(ApplicationDbContext context,
            IFtpService ftpService,
            IHttpClientService httpClientService,
            IClaimsInvestigationService claimsInvestigationService,
            IMailboxService mailboxService,
            UserManager<ClientCompanyApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<ApplicationRole> roleManager,
            IToastNotification toastNotification)
        {
            _context = context;
            this.ftpService = ftpService;
            this.httpClientService = httpClientService;
            this.claimsInvestigationService = claimsInvestigationService;
            this.mailboxService = mailboxService;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this.roleManager = roleManager;
            this.toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Breadcrumb(" Upload Log", FromController = typeof(ClaimsInvestigationController))]
        public async Task<IActionResult> Uploads()
        {
            var userEmail = HttpContext.User.Identity.Name;

            var fileuploadViewModel = await LoadAllFiles(userEmail);
            ViewBag.Message = TempData["Message"];
            return View(fileuploadViewModel);
        }

        public async Task<IActionResult> DownloadLog(int id)
        {
            var file = await _context.FilesOnFileSystem.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null) return null;
            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, file.FileType, file.Name + file.Extension);
        }

        public async Task<IActionResult> DeleteLog(int id)
        {
            var file = await _context.FilesOnFileSystem.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null) return null;
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
            _context.FilesOnFileSystem.Remove(file);
            _context.SaveChanges();
            TempData["Message"] = $"Removed {file.Name + file.Extension} successfully from File System.";
            return RedirectToAction("Uploads");
        }

        private async Task<FileUploadViewModel> LoadAllFiles(string userEmail)
        {
            var viewModel = new FileUploadViewModel();
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(u => u.Email == userEmail);

            var company = _context.ClientCompany.FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            viewModel.FilesOnFileSystem = await _context.FilesOnFileSystem.Where(f => f.CompanyId == company.ClientCompanyId).ToListAsync();
            return viewModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FtpDownload()
        {
            try
            {
                var userEmail = HttpContext.User.Identity.Name;

                await ftpService.DownloadFtp(userEmail);

                toastNotification.AddSuccessToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> Ftp Downloaded Claims ready"));

                return RedirectToAction("Draft", "ClaimsInvestigation");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                toastNotification.AddErrorToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> Ftp Downloaded err !!!"));
                return RedirectToAction("Draft", "ClaimsInvestigation");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FtpUpload(IFormFile postedFtp)
        {
            if (postedFtp != null)
            {
                string folder = Path.Combine(webHostEnvironment.WebRootPath, "document");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName = Path.GetFileName(postedFtp.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    postedFtp.CopyTo(stream);
                }
                var wc = new WebClient
                {
                    Credentials = new NetworkCredential(Applicationsettings.FTP_SITE_LOG, Applicationsettings.FTP_SITE_DATA),
                };
                var response = wc.UploadFile(Applicationsettings.FTP_SITE + fileName, filePath);

                var data = Encoding.UTF8.GetString(response);

                var userEmail = HttpContext.User.Identity.Name;

                SaveUpload(postedFtp, filePath, "Ftp upload", userEmail);

                toastNotification.AddSuccessToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> Ftp Uploaded Claims."));

                return RedirectToAction("Draft", "ClaimsInvestigation");
            }
            return Problem();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UploadClaims(IFormFile postedFile)
        {
            if (postedFile != null && Path.GetExtension(postedFile.FileName) == ".zip")
            {
                string path = Path.Combine(webHostEnvironment.WebRootPath, "upload-file");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string docPath = Path.Combine(webHostEnvironment.WebRootPath, "upload-case");
                if (!Directory.Exists(docPath))
                {
                    Directory.CreateDirectory(docPath);
                }
                string fileName = Path.GetTempFileName();
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(postedFile.FileName);
                string filePath = Path.Combine(path, fileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                var userEmail = HttpContext.User.Identity.Name;

                await ftpService.UploadFile(userEmail, filePath, docPath, fileNameWithoutExtension);

                SaveUpload(postedFile, filePath, "File upload", userEmail);
                try
                {
                    var rows = _context.SaveChanges();

                    toastNotification.AddSuccessToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> File uploaded Claims ready"));

                    return RedirectToAction("Draft", "ClaimsInvestigation");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            toastNotification.AddErrorToastMessage(string.Format("<i class='far fa-file-powerpoint'></i> File uploaded err "));

            return RedirectToAction("Draft", "ClaimsInvestigation");
        }

        private void SaveUpload(IFormFile file, string filePath, string description, string uploadedBy)
        {
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var company = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == uploadedBy);
            var fileModel = new FileOnFileSystemModel
            {
                CreatedOn = DateTime.UtcNow,
                FileType = file.ContentType,
                Extension = extension,
                Name = fileName,
                Description = description,
                FilePath = filePath,
                UploadedBy = uploadedBy,
                CompanyId = company.ClientCompanyId
            };
            _context.FilesOnFileSystem.Add(fileModel);
            _context.SaveChanges();
        }
    }
}