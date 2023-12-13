using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using risk.control.system.Data;
using risk.control.system.Models;

namespace risk.control.system.Controllers.Api
{
    public class MasterDataController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public MasterDataController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost, ActionName("GetStatesByCountryId")]
        public async Task<IActionResult> GetStatesByCountryId(string countryId)
        {
            string cId;
            var states = new List<State>();
            if (!string.IsNullOrEmpty(countryId))
            {
                cId = countryId;
                states = await context.State.Where(s => s.CountryId.Equals(cId)).OrderBy(s => s.Code).ToListAsync();
            }
            return Ok(states);
        }

        [HttpPost, ActionName("GetDistrictByStateId")]
        public async Task<IActionResult> GetDistrictByStateId(string stateId)
        {
            string sId;
            var districts = new List<District>();
            if (!string.IsNullOrEmpty(stateId))
            {
                sId = stateId;
                districts = await context.District.Where(s => s.State.StateId.Equals(sId)).OrderBy(s => s.Code).ToListAsync();
            }
            return Ok(districts);
        }

        [HttpPost, ActionName("GetPinCodesByDistrictId")]
        public async Task<IActionResult> GetPinCodesByDistrictId(string districtId)
        {
            string sId;
            var pincodes = new List<PinCode>();
            if (!string.IsNullOrEmpty(districtId))
            {
                sId = districtId;
                pincodes = await context.PinCode.Where(s => s.District.DistrictId.Equals(sId)).OrderBy(s => s.Code).ToListAsync();
            }
            return Ok(pincodes);
        }

        [HttpPost, ActionName("GetPincodesByDistrictIdWithoutPreviousSelected")]
        public async Task<IActionResult> GetPincodesByDistrictIdWithoutPreviousSelected(string districtId, string caseId)
        {
            string sId;
            var pincodes = new List<PinCode>();
            var remaingPincodes = new List<PinCode>();

            if (!string.IsNullOrEmpty(districtId))
            {
                sId = districtId;
                pincodes = await context.PinCode.Where(s => s.District.DistrictId.Equals(sId)).OrderBy(s => s.Code).ToListAsync();

                //var existingCaseLocations = context.CaseLocation
                //    .Include(c => c.PincodeServices)
                //    .Where(c => c.ClaimsInvestigationId == caseId);

                //var existingVerifyPincodes = context.VerifyPinCode
                //    .Where(v => existingCaseLocations.Any(e => e.CaseLocationId == v.CaseLocationId))?.ToList();

                //if (existingVerifyPincodes is not null && existingVerifyPincodes.Any())
                //{
                //    var existingPicodes = existingVerifyPincodes.Select(e => e.Pincode).ToList();
                //    var pinCodeString = pincodes.Select(p=>p.Code).ToList();
                //    var remaingPincodesString = pinCodeString.Except(existingPicodes).ToList();
                //    remaingPincodes = pincodes.Where(p=> remaingPincodesString.Contains(p.Code)).ToList();
                //    return Ok(remaingPincodes);
                //}
            }
            return Ok(pincodes);
        }

        [HttpPost, ActionName("GetPincodesByDistrictIdWithoutPreviousSelectedService")]
        public async Task<IActionResult> GetPincodesByDistrictIdWithoutPreviousSelectedService(string districtId, string vendorId, string lobId, string serviceId)
        {
            string sId;
            var pincodes = new List<PinCode>();
            var remaingPincodes = new List<PinCode>();

            if (!string.IsNullOrEmpty(districtId))
            {
                sId = districtId;
                pincodes = await context.PinCode.Where(s => s.District.DistrictId.Equals(sId)).ToListAsync();

                var vendor = context.Vendor
                    .Include(c => c.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.Country)
                    .Include(c => c.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.State)
                    .Include(c => c.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.District)
                    .Include(c => c.VendorInvestigationServiceTypes)
                    .ThenInclude(v => v.PincodeServices)
                    .FirstOrDefault(c => c.VendorId == vendorId);

                var existingVendorServices = vendor.VendorInvestigationServiceTypes;

                var existingServicedPincodes = new List<ServicedPinCode>();

                if (existingVendorServices is not null && existingVendorServices.Any())
                {
                    var pinCodeString = pincodes.Select(p => p.Code).ToList();

                    foreach (var existingVendorService in existingVendorServices)
                    {
                        if (existingVendorService.LineOfBusinessId == lobId && existingVendorService.InvestigationServiceTypeId == serviceId)
                        {
                            foreach (var pincodeService in existingVendorService.PincodeServices)
                            {
                                existingServicedPincodes.Add(pincodeService);
                            }
                        }
                    }

                    var existingPicodes = existingServicedPincodes.Select(e => e.Pincode).ToList();
                    var remaingPincodesString = pinCodeString.Except(existingPicodes).ToList();
                    remaingPincodes = pincodes.Where(p => remaingPincodesString.Contains(p.Code)).OrderBy(s => s.Code).ToList();
                    return Ok(remaingPincodes);
                }
            }
            return Ok(pincodes);
        }

        [HttpPost, ActionName("GetUserBySearch")]
        public async Task<IActionResult> GetUserBySearch(string search)
        {
            var applicationUsers = new List<ApplicationUser>();
            if (!string.IsNullOrEmpty(search))
            {
                applicationUsers = await context.ApplicationUser.Where(s =>
                   (!string.IsNullOrEmpty(search) && s.Email.ToLower().Contains(search.Trim().ToLower()))
                   || true
                ).ToListAsync();
            }
            return Ok(applicationUsers?.Select(a => a.Email).OrderBy(s => s).ToList());
        }
    }
}