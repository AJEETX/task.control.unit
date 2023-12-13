using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Helpers;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;

namespace risk.control.system.Controllers.Api.Company
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ClientCompanyApplicationUser> userManager;

        public CompanyController(ApplicationDbContext context, UserManager<ClientCompanyApplicationUser> userManager)
        {
            this.userManager = userManager;
            _context = context;
        }

        [HttpGet("AllCompanies")]
        public async Task<IActionResult> AllCompanies()
        {
            var companies = await _context.ClientCompany
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.District)
                .Include(v => v.State)
                .ToListAsync();
            var result =
                companies.Select(u =>
                new
                {
                    Id = u.ClientCompanyId,
                    Document = u.DocumentUrl,
                    Domain = "<a href=''>" + u.Email + "</a>",
                    Name = u.Name,
                    Code = u.Code,
                    Phone = u.PhoneNumber,
                    Address = u.Addressline,
                    District = u.District.Name,
                    State = u.State.Name,
                    Country = u.Country.Name
                });

            return Ok(result.ToArray());
        }

        [HttpGet("CompanyUsers")]
        public async Task<IActionResult> CompanyUsers(string id)
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var adminUser = _context.ApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            if (!adminUser.IsSuperAdmin)
            {
                return BadRequest();
            }

            var company = _context.ClientCompany
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.PinCode)
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.Country)
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.District)
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.State)
                .FirstOrDefault(c => c.ClientCompanyId == id);

            var users = company.CompanyApplicationUser
                .Where(u => !u.Deleted)
                .AsQueryable();
            var result =
                users.Select(u =>
                new
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = "<a href=''>" + u.Email + "</a>",
                    Phone = u.PhoneNumber,
                    Photo = u.ProfilePictureUrl,
                    Active = u.Active,
                    Addressline = u.Addressline,
                    District = u.District.Name,
                    State = u.State.Name,
                    Country = u.Country.Name,
                    Roles = string.Join(",", GetUserRoles(u).Result),
                    Pincode = u.PinCode.Code,
                });
            await Task.Delay(1000);

            return Ok(result.ToArray());
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> AllUsers()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var company = _context.ClientCompany
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.PinCode)
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.Country)
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.District)
                .Include(c => c.CompanyApplicationUser)
                .ThenInclude(u => u.State)
                .FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            var users = company.CompanyApplicationUser
                .Where(u => !u.Deleted)
                .AsQueryable();
            var result =
                users.Select(u =>
                new
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = "<a href=''>" + u.Email + "</a>",
                    Phone = u.PhoneNumber,
                    Photo = u.ProfilePictureUrl,
                    Active = u.Active,
                    Addressline = u.Addressline,
                    District = u.District.Name,
                    State = u.State.Name,
                    Country = u.Country.Name,
                    Roles = string.Join(",", GetUserRoles(u).Result),
                    Pincode = u.PinCode.Code,
                });
            await Task.Delay(1000);

            return Ok(result.ToArray());
        }

        [HttpGet("GetEmpanelledVendors")]
        public async Task<IActionResult> GetEmpanelledVendors()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var company = _context.ClientCompany
                .Include(c => c.CompanyApplicationUser)
                .Include(c => c.EmpanelledVendors)
                .ThenInclude(c => c.State)
                .Include(c => c.EmpanelledVendors)
                .ThenInclude(c => c.District)
                .Include(c => c.EmpanelledVendors)
                .ThenInclude(c => c.Country)
                .FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            var result =
                company.EmpanelledVendors.Where(v => !v.Deleted).Select(u =>
                new
                {
                    Id = u.VendorId,
                    Document = u.DocumentUrl,
                    Domain = "<a href=''>" + u.Email + "</a>",
                    Name = u.Name,
                    Code = u.Code,
                    Phone = u.PhoneNumber,
                    Address = u.Addressline,
                    District = u.District.Name,
                    State = u.State.Name,
                    Country = u.Country.Name
                });

            return Ok(result.ToArray());
        }

        [HttpGet("GetAvailableVendors")]
        public async Task<IActionResult> GetAvailableVendors()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var companyUser = _context.ClientCompanyApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var company = _context.ClientCompany
                .Include(c => c.CompanyApplicationUser)
                .FirstOrDefault(c => c.ClientCompanyId == companyUser.ClientCompanyId);

            var availableVendors = _context.Vendor
                .Where(v =>
                !v.Clients.Any(c => c.ClientCompanyId == companyUser.ClientCompanyId) &&
                (v.VendorInvestigationServiceTypes != null) && v.VendorInvestigationServiceTypes.Count > 0)
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.District)
                .Include(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.District)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.LineOfBusiness)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.InvestigationServiceType)
                .Include(v => v.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.PincodeServices)
                .Where(v => !v.Deleted)
                .AsQueryable();

            var result =
                availableVendors.Select(u =>
                new
                {
                    Id = u.VendorId,
                    Document = u.DocumentUrl,
                    Domain = "<a href=''>" + u.Email + "</a>",
                    Name = u.Name,
                    Code = u.Code,
                    Phone = u.PhoneNumber,
                    Address = u.Addressline,
                    District = u.District.Name,
                    State = u.State.Name,
                    Country = u.Country.Name
                });
            return Ok(result);
        }

        [HttpGet("AllServices")]
        public async Task<IActionResult> AllServices(string id)
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var vendor = _context.Vendor
                .Include(i => i.VendorInvestigationServiceTypes)
                .ThenInclude(i => i.LineOfBusiness)
                .Include(i => i.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.District)
                 .Include(i => i.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.State)
                .Include(i => i.VendorInvestigationServiceTypes)
                .ThenInclude(v => v.Country)
                .Include(i => i.District)
                .Include(i => i.VendorInvestigationServiceTypes)
                .ThenInclude(i => i.InvestigationServiceType)
                .Include(i => i.State)
                .Include(i => i.VendorInvestigationServiceTypes)
                .ThenInclude(i => i.PincodeServices)
                .FirstOrDefault(a => a.VendorId == id);

            var result = vendor.VendorInvestigationServiceTypes.Select(s => new
            {
                VendorId = s.VendorId,
                Id = s.VendorInvestigationServiceTypeId,
                CaseType = s.LineOfBusiness.Name,
                ServiceType = s.InvestigationServiceType.Name,
                District = s.District.Name,
                State = s.State.Name,
                Country = s.Country.Name,
                Pincodes = s.PincodeServices.Count == 0 ?
                    "<span class=\"badge badge-danger\"><img class=\"timer-image\" src=\"/img/timer.gif\" /> </span>" :
                     string.Join("", s.PincodeServices.Select(c => "<span class='badge badge-light'>" + c.Pincode + "</span> ")),
                Rate = s.Price,
                UpdatedBy = s.UpdatedBy,
            });

            return Ok(result);
        }

        private async Task<List<string>> GetUserRoles(ClientCompanyApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            var decoratedRoles = new List<string>();

            foreach (var role in roles)
            {
                var decoratedRole = "<span class=\"badge badge-danger\">" + role + "</span>";
                decoratedRoles.Add(decoratedRole);
            }
            return decoratedRoles;
        }
    }
}