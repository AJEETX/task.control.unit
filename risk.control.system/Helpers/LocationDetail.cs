using System;

using risk.control.system.Models;

namespace risk.control.system.Helpers
{
    public class LocationDetail
    {
        private static Random random = new Random();

        public static (decimal, decimal) GetLatLng(decimal lat, decimal lng)
        {
            //Earth’s radius
            var R = 6378137.0;

            //offsets in meters (random values between 3 and 5)
            var DistanceNorth = random.Next(30, 50);
            var DistanceEast = random.Next(30, 50);

            //Coordinate offsets in radians
            var dLat = DistanceNorth / R;
            var dLon = DistanceEast / (R * Math.Cos(Math.PI * (double.Parse(lat.ToString("###.#######"))) / 180));

            //New coordinates
            var tmpLat = dLat * 180 / Math.PI;
            var NewLat = lat + decimal.Parse(tmpLat.ToString("###.#######"));
            var tmpLng = dLon * 180 / Math.PI;
            var NewLng = lng + decimal.Parse(tmpLng.ToString("###.#######"));
            return (NewLat, NewLng);
        }

        public static string GetAddress(ClaimType? claimType, CustomerDetail a, CaseLocation location)
        {
            if (claimType == ClaimType.HEALTH)
            {
                if (a is null)
                    return string.Empty;
                return a.Addressline + " " + a.District?.Name + " " + a.State?.Name + " " + a.Country?.Name + " " + a.PinCode?.Code;
            }
            else
            {
                if (location is null)
                    return string.Empty;
                return location.Addressline + " " + location.District.Name + " " + location.State.Name + " " + location.Country.Name + " " + location.PinCode.Code;
            }
        }
    }
}