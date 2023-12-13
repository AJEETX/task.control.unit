using CsvHelper.Configuration;
using risk.control.system.Models.ViewModel;
using risk.control.system.Models;
using System.Globalization;
using CsvHelper;
using risk.control.system.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;

namespace risk.control.system.Seeds
{
    public static class PinCodeStateSeed
    {
        private static string stateWisePincodeFilePath = @"au_postcodes.csv";

        //private static string stateWisePincodeFilePath = @"pincode.csv";
        private static string NO_DATA = " NO - DATA ";

        private static Regex regex = new Regex("\\\"(.*?)\\\"");

        public static async Task SeedPincode(ApplicationDbContext context, Country country)
        {
            var pincodes = await CsvRead();

            // add the states with pincodes
            var states = pincodes.GroupBy(g => new { g.StateName, g.StateCode });
            foreach (var state in states)
            {
                var recordState = new State { Code = state.Key.StateCode, Name = state.Key.StateName, CountryId = country.CountryId };
                var stateAdded = await context.State.AddAsync(recordState);

                var districts = state.GroupBy(g => g.District);

                var pinCodeList = new List<PinCode> { };
                foreach (var district in districts)
                {
                    var districtDetail = new District { Code = district.Key, Name = district.Key, StateId = stateAdded.Entity.StateId, CountryId = country.CountryId };
                    var districtAdded = await context.District.AddAsync(districtDetail);
                    foreach (var pinCode in district)
                    {
                        var pincodeState = new PinCode
                        {
                            Name = pinCode.Name,
                            Code = pinCode.Code,
                            Longitude = pinCode.Longitude,
                            Latitude = pinCode.Latitude,
                            DistrictId = districtAdded.Entity.DistrictId,
                            StateId = stateAdded.Entity.StateId,
                            CountryId = country.CountryId,
                        };
                        pinCodeList.Add(pincodeState);
                    }
                }
                await context.PinCode.AddRangeAsync(pinCodeList);
            }
        }

        private static async Task<List<PinCodeState>> CsvRead()
        {
            var pincodes = new List<PinCodeState>();
            string csvData = await File.ReadAllTextAsync(stateWisePincodeFilePath);

            bool firstRow = true;
            foreach (string row in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(row))
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        if (firstRow)
                        {
                            firstRow = false;
                        }
                        else
                        {
                            var output = regex.Replace(row, m => m.Value.Replace(',', '@'));
                            var rowData = output.Split(',').ToList();
                            var pincodeState = new PinCodeState
                            {
                                Code = rowData[0] ?? NO_DATA,
                                Name = rowData[1] ?? NO_DATA,
                                District = rowData[1] ?? NO_DATA,
                                StateName = rowData[2] ?? NO_DATA,
                                StateCode = rowData[3] ?? NO_DATA,
                                Latitude = rowData[4] ?? NO_DATA,
                                Longitude = rowData[5] ?? NO_DATA,
                            };
                            var isDupicate = pincodes.FirstOrDefault(p => p.Code == pincodeState.Code);
                            if (isDupicate is null)
                            {
                                pincodes.Add(pincodeState);
                            }
                        }
                    }
                }
            }
            var smallerPincodes = pincodes.Where(p => p.StateCode == "VIC" || p.StateCode == "NSW")?.ToList();
            return smallerPincodes.Distinct()?.ToList();
        }
    }
}