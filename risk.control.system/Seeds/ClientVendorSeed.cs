using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using MimeKit.Encodings;

using risk.control.system.AppConstant;
using risk.control.system.Data;
using risk.control.system.Models;
using risk.control.system.Services;

namespace risk.control.system.Seeds
{
    public class ClientVendorSeed
    {
        public static async Task<(Vendor checker, Vendor verify, Vendor investigate, string canaraId, string hdfcId)> Seed(ApplicationDbContext context, EntityEntry<Country> indiaCountry,
            InvestigationServiceType investigationServiceType, InvestigationServiceType discreetServiceType, InvestigationServiceType docServiceType, LineOfBusiness lineOfBusiness, IHttpClientService httpClientService)
        {
            var companyPinCode = context.PinCode.Include(p => p.District).FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE);
            var companyDistrict = context.District.Include(d => d.State).FirstOrDefault(s => s.DistrictId == companyPinCode.District.DistrictId);
            var companyStateId = context.State.FirstOrDefault(s => s.StateId == companyDistrict.State.StateId)?.StateId ?? default!;

            //var pinCodeData = await httpClientService.GetPinCodeLatLng(companyPinCode.Code);

            //companyPinCode.Latitude = pinCodeData.FirstOrDefault()?.Lat.ToString();
            //companyPinCode.Longitude = pinCodeData.FirstOrDefault()?.Lng.ToString();

            //CREATE VENDOR COMPANY

            var checkerPinCode = context.PinCode.Include(p => p.District).FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE2);
            var checkerDistrict = context.District.Include(d => d.State).FirstOrDefault(s => s.DistrictId == checkerPinCode.District.DistrictId);
            var checkerState = context.State.FirstOrDefault(s => s.StateId == checkerDistrict.State.StateId);

            var checker = new Vendor
            {
                Name = Applicationsettings.AGENCY1NAME,
                Addressline = "1, Nice Road  ",
                Branch = "MAHATTAN",
                Code = Applicationsettings.AGENCY1CODE,
                ActivatedDate = DateTime.Now,
                AgreementDate = DateTime.Now,
                BankName = "WESTPAC",
                BankAccountNumber = "1234567",
                IFSCCode = "IFSC100",
                CountryId = indiaCountry.Entity.CountryId,
                DistrictId = checkerDistrict.DistrictId,
                StateId = checkerState.StateId,
                PinCodeId = checkerPinCode.PinCodeId,
                Description = "HEAD OFFICE ",
                Email = Applicationsettings.AGENCY1DOMAIN,
                PhoneNumber = "8888004739",
                DocumentUrl = "/img/checker.png"
            };

            var checkerAgency = await context.Vendor.AddAsync(checker);

            var verifyPinCode = context.PinCode.Include(p => p.District).FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE3);
            var verifyDistrict = context.District.Include(d => d.State).FirstOrDefault(s => s.DistrictId == verifyPinCode.District.DistrictId);
            var verifyState = context.State.FirstOrDefault(s => s.StateId == verifyDistrict.State.StateId);

            var verify = new Vendor
            {
                Name = Applicationsettings.AGENCY2NAME,
                Addressline = "10, Clear Road  ",
                Branch = "BLACKBURN",
                Code = Applicationsettings.AGENCY2CODE,
                ActivatedDate = DateTime.Now,
                AgreementDate = DateTime.Now,
                BankName = "SBI BANK",
                BankAccountNumber = "9876543",
                IFSCCode = "IFSC999",
                CountryId = indiaCountry.Entity.CountryId,
                DistrictId = companyDistrict.DistrictId,
                StateId = companyStateId,
                PinCodeId = companyPinCode.PinCodeId,
                Description = "HEAD OFFICE ",
                Email = Applicationsettings.AGENCY2DOMAIN,
                PhoneNumber = "4444404739",
                DocumentUrl = "/img/verify.png"
            };

            var verifyAgency = await context.Vendor.AddAsync(verify);

            var investigatePinCode = context.PinCode.Include(p => p.District).FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE4);
            var investigateDistrict = context.District.Include(d => d.State).FirstOrDefault(s => s.DistrictId == investigatePinCode.District.DistrictId);
            var investigateState = context.State.FirstOrDefault(s => s.StateId == investigateDistrict.State.StateId);

            var investigate = new Vendor
            {
                Name = Applicationsettings.AGENCY3NAME,
                Addressline = "1, Main Road  ",
                Branch = "CLAYTON ROAD",
                Code = Applicationsettings.AGENCY3CODE,
                ActivatedDate = DateTime.Now,
                AgreementDate = DateTime.Now,
                BankName = "HDFC BANK",
                BankAccountNumber = "9876543",
                IFSCCode = "IFSC999",
                CountryId = indiaCountry.Entity.CountryId,
                DistrictId = investigateDistrict.DistrictId,
                StateId = investigateState.StateId,
                PinCodeId = investigatePinCode.PinCodeId,
                Description = "HEAD OFFICE ",
                Email = Applicationsettings.AGENCY3DOMAIN,
                PhoneNumber = "7964404160",
                DocumentUrl = "/img/investigate.png"
            };

            var investigateAgency = await context.Vendor.AddAsync(investigate);

            //CREATE COMPANY1

            var canara = new ClientCompany
            {
                ClientCompanyId = Guid.NewGuid().ToString(),
                Name = Applicationsettings.BSNL,
                Addressline = "34 Lasiandra Avenue ",
                Branch = "FOREST HILL CHASE",
                Code = Applicationsettings.BSNL_CODE,
                ActivatedDate = DateTime.Now,
                AgreementDate = DateTime.Now,
                BankName = "NAB",
                BankAccountNumber = "1234567",
                IFSCCode = "IFSC100",
                CountryId = indiaCountry.Entity.CountryId,
                DistrictId = companyDistrict.DistrictId,
                StateId = companyStateId,
                PinCodeId = companyPinCode.PinCodeId,
                Description = "CORPORATE OFFICE ",
                Email = Applicationsettings.BSNL_DOMAIN,
                DocumentUrl = Applicationsettings.BSNL_LOGO,
                PhoneNumber = "9988004739",
                EmpanelledVendors = new List<Vendor> { checker, verify, investigate }
            };

            var canaraCompany = await context.ClientCompany.AddAsync(canara);

            //CREATE COMPANY2

            var hdfc = new ClientCompany
            {
                ClientCompanyId = Guid.NewGuid().ToString(),
                Name = Applicationsettings.HDFC,
                Addressline = "34 Lasiandra Avenue ",
                Branch = "FOREST HILL CHASE",
                Code = Applicationsettings.HDFCCODE,
                ActivatedDate = DateTime.Now,
                AgreementDate = DateTime.Now,
                BankName = "NAB",
                BankAccountNumber = "1234567",
                IFSCCode = "IFSC100",
                CountryId = indiaCountry.Entity.CountryId,
                DistrictId = companyDistrict.DistrictId,
                StateId = companyStateId,
                PinCodeId = companyPinCode.PinCodeId,
                Description = "CORPORATE OFFICE ",
                Email = Applicationsettings.HDFCDOMAIN,
                DocumentUrl = Applicationsettings.HDFCLOGO,
                PhoneNumber = "9988004739",
                EmpanelledVendors = new List<Vendor> { checker, verify, investigate }
            };

            var hdfcCompany = await context.ClientCompany.AddAsync(hdfc);

            var checkerServices = new List<VendorInvestigationServiceType>
            {
                new VendorInvestigationServiceType{
                    VendorId = checkerAgency.Entity.VendorId,
                    InvestigationServiceTypeId = investigationServiceType.InvestigationServiceTypeId,
                    Price = 199,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    CountryId = indiaCountry.Entity.CountryId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                },
                new VendorInvestigationServiceType{
                    VendorId = checkerAgency.Entity.VendorId,
                    InvestigationServiceTypeId = docServiceType.InvestigationServiceTypeId,
                    Price = 99,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    CountryId = indiaCountry.Entity.CountryId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                }
            };

            var verifyServices = new List<VendorInvestigationServiceType>
            {
                new VendorInvestigationServiceType{
                    VendorId = verifyAgency.Entity.VendorId,
                    InvestigationServiceTypeId = investigationServiceType.InvestigationServiceTypeId,
                    Price = 399,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    CountryId = indiaCountry.Entity.CountryId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                },
                new VendorInvestigationServiceType{
                    VendorId = verifyAgency.Entity.VendorId,
                    InvestigationServiceTypeId = discreetServiceType.InvestigationServiceTypeId,
                    Price = 299,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    CountryId = indiaCountry.Entity.CountryId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                }
            };

            var investigateServices = new List<VendorInvestigationServiceType>
            {
                new VendorInvestigationServiceType{
                    VendorId = investigateAgency.Entity.VendorId,
                    InvestigationServiceTypeId = docServiceType.InvestigationServiceTypeId,
                    Price = 199,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    CountryId = indiaCountry.Entity.CountryId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                },
                new VendorInvestigationServiceType{
                    VendorId = investigateAgency.Entity.VendorId,
                    InvestigationServiceTypeId = discreetServiceType.InvestigationServiceTypeId,
                    Price = 299,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    CountryId = indiaCountry.Entity.CountryId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                },
                new VendorInvestigationServiceType{
                    VendorId = investigateAgency.Entity.VendorId,
                    InvestigationServiceTypeId = investigationServiceType.InvestigationServiceTypeId,
                    Price = 599,
                    DistrictId = companyDistrict.DistrictId,
                    StateId = companyStateId,
                    CountryId = indiaCountry.Entity.CountryId,
                    LineOfBusinessId = lineOfBusiness.LineOfBusinessId,
                    PincodeServices = new List<ServicedPinCode>
                    {
                        new ServicedPinCode
                        {
                            Pincode = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Code ?? default !,
                            Name = context.PinCode.FirstOrDefault(s => s.Code == Applicationsettings.CURRENT_PINCODE)?.Name ?? default !
                        }
                    }
                }
            };

            checker.VendorInvestigationServiceTypes = checkerServices;
            verify.VendorInvestigationServiceTypes = verifyServices;
            investigate.VendorInvestigationServiceTypes = investigateServices;

            checker.Clients.Add(canaraCompany.Entity);
            verify.Clients.Add(canaraCompany.Entity);
            investigate.Clients.Add(canaraCompany.Entity);

            checker.Clients.Add(hdfcCompany.Entity);
            verify.Clients.Add(hdfcCompany.Entity);
            investigate.Clients.Add(hdfcCompany.Entity);

            await context.SaveChangesAsync(null, false);
            return (checker, verify, investigate, canaraCompany.Entity.ClientCompanyId, hdfcCompany.Entity.ClientCompanyId);
        }
    }
}