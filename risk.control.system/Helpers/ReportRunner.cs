using Gehtsoft.PDFFlow.Builder;

using Newtonsoft.Json;

using risk.control.system.Models.ViewModel;

namespace risk.control.system.Helpers
{
    public static class ReportRunner
    {
        public static DocumentBuilder Run(string imagePath)
        {
            string ticketJsonFile = CheckFile(
                Path.Combine("Files", "ticket-data.json"));
            string routeJsonFile = CheckFile(
                Path.Combine("Files", "route-data.json"));
            string tripJsonFile = CheckFile(
                Path.Combine("Files", "about-trip.json"));
            string fareJsonFile = CheckFile(
                Path.Combine("Files", "fare-breakdown.json"));
            string helpJsonFile = CheckFile(
                Path.Combine("Files", "help-list.json"));
            string ticketJsonContent = File.ReadAllText(ticketJsonFile);
            string routeJsonContent = File.ReadAllText(routeJsonFile);
            string tripJsonContent = File.ReadAllText(tripJsonFile);
            string fareJsonContent = File.ReadAllText(fareJsonFile);
            string helpJsonContent = File.ReadAllText(helpJsonFile);
            Summarydata ticketData =
                JsonConvert.DeserializeObject<Summarydata>(ticketJsonContent);
            List<InvestigationData> routeData =
                JsonConvert.DeserializeObject<List<InvestigationData>>(routeJsonContent);
            List<string> tripData =
                JsonConvert.DeserializeObject<List<string>>(tripJsonContent);
            List<CustomerData> fareData =
                JsonConvert.DeserializeObject<List<CustomerData>>(fareJsonContent);
            List<BeneficiaryData> helpData =
                JsonConvert.DeserializeObject<List<BeneficiaryData>>(helpJsonContent);
            ReportBuilder airplaneTicketBuilder =
                new ReportBuilder();
            airplaneTicketBuilder.TicketData = ticketData;
            airplaneTicketBuilder.RouteData = routeData;
            airplaneTicketBuilder.TripData = tripData;
            airplaneTicketBuilder.FareData = fareData;
            airplaneTicketBuilder.HelpData = helpData;
            return airplaneTicketBuilder.Build(imagePath);
        }

        private static string CheckFile(string file)
        {
            if (!File.Exists(file))
            {
                throw new IOException("File not found: " + Path.GetFullPath(file));
            }
            return file;
        }
    }
}