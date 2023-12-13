using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

namespace risk.control.system.Controllers.Api.Agency
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<VendorApplicationUser> userManager;
        private readonly IDashboardService dashboardService;

        public AgencyController(ApplicationDbContext context, UserManager<VendorApplicationUser> userManager, IDashboardService dashboardService)
        {
            this.userManager = userManager;
            this.dashboardService = dashboardService;
            _context = context;
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> AllUsers()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);

            var vendor = _context.Vendor
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.District)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.State)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.Country)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.PinCode)
                .FirstOrDefault(c => c.VendorId == vendorUser.VendorId);

            var users = vendor.VendorApplicationUser.Where(u => !u.Deleted).AsQueryable();
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
                    Pincode = u.PinCode.Code,
                    Roles = string.Join(",", GetUserRoles(u).Result)
                });

            return Ok(result.ToArray());
        }

        [HttpGet("AllAgencies")]
        public async Task<IActionResult> AllAgencies()
        {
            var agencies = await _context.Vendor
                .Include(v => v.Country)
                .Include(v => v.PinCode)
                .Include(v => v.District)
                .Include(v => v.State)
                .Include(v => v.VendorInvestigationServiceTypes)
                .Where(v => !v.Deleted)
                .ToListAsync();
            var result =
                agencies.Select(u =>
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

        [HttpGet("AllServices")]
        public async Task<IActionResult> AllServices()
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
                .FirstOrDefault(a => a.VendorId == vendorUser.VendorId && !a.Deleted);

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

        [HttpGet("GetCompanyAgencyUser")]
        public async Task<IActionResult> GetCompanyAgencyUser(string id)
        {
            var vendor = _context.Vendor
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.District)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.State)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.Country)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.PinCode)
                .FirstOrDefault(c => c.VendorId == id && !c.Deleted);

            var users = vendor.VendorApplicationUser.AsQueryable();
            var result =
                users.Select(u =>
                new
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = "<a href=''>" + u.Email + "</a>",
                    Phone = u.PhoneNumber,
                    Photo = u.ProfilePictureUrl,
                    Addressline = u.Addressline,
                    Active = u.Active,
                    District = u.District.Name,
                    State = u.State.Name,
                    Country = u.Country.Name,
                    Pincode = u.PinCode.Code,
                    Roles = string.Join(",", GetUserRoles(u).Result)
                });

            return Ok(result.ToArray());
        }

        [HttpGet("GetAgentLoad")]
        public async Task<IActionResult> GetAgentLoad()
        {
            var userEmail = HttpContext.User?.Identity?.Name;
            var vendorUser = _context.VendorApplicationUser.FirstOrDefault(c => c.Email == userEmail);
            var adminRole = _context.ApplicationRole.FirstOrDefault(r => r.Name.Contains(AppRoles.AgencyAdmin.ToString()));
            List<VendorUserClaim> agents = new List<VendorUserClaim>();

            var vendor = _context.Vendor
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.PinCode)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.State)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.District)
                .Include(c => c.VendorApplicationUser)
                .ThenInclude(u => u.Country)
                .FirstOrDefault(c => c.VendorId == vendorUser.VendorId);

            var users = vendor.VendorApplicationUser.AsQueryable();
            var result = dashboardService.CalculateAgentCaseStatus(userEmail);

            foreach (var user in users)
            {
                var isAdmin = await userManager.IsInRoleAsync(user, adminRole?.Name);
                if (!isAdmin)
                {
                    int claimCount = 0;
                    if (result.TryGetValue(user.Email, out claimCount))
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = user,
                            CurrentCaseCount = claimCount,
                        };
                        agents.Add(agentData);
                    }
                    else
                    {
                        var agentData = new VendorUserClaim
                        {
                            AgencyUser = user,
                            CurrentCaseCount = 0,
                        };
                        agents.Add(agentData);
                    }
                }
            }
            var agentWithLoad = agents.Select(u => new
            {
                Id = u.AgencyUser.Id,
                Photo = u.AgencyUser.ProfilePictureUrl,
                Email = "<a href=''>" + u.AgencyUser.Email + "</a>",
                Name = u.AgencyUser.FirstName + " " + u.AgencyUser.LastName,
                Phone = u.AgencyUser.PhoneNumber,
                Addressline = u.AgencyUser.Addressline,
                District = u.AgencyUser.District.Name,
                State = u.AgencyUser.State.Name,
                Country = u.AgencyUser.Country.Name,
                Pincode = u.AgencyUser.PinCode.Code,
                Active = u.AgencyUser.Active,
                Roles = string.Join(",", GetUserRoles(u.AgencyUser).Result),
                Count = u.CurrentCaseCount
            });
            return Ok(agentWithLoad);
        }

        private async Task<List<string>> GetUserRoles(VendorApplicationUser user)
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