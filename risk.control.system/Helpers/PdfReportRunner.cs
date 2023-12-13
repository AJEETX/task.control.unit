using Gehtsoft.PDFFlow.Builder;

using Newtonsoft.Json;

using risk.control.system.Models.ViewModel;

namespace risk.control.system.Helpers
{
    public class PdfReportRunner
    {
        public static DocumentBuilder Run(string imagePath)
        {
            string ticketJsonFile = CheckFile(Path.Combine("Files", "concert-ticket-data.json"));
            string ticketJsonContent = File.ReadAllText(ticketJsonFile);
            TicketData ticketData =
               JsonConvert.DeserializeObject<TicketData>(ticketJsonContent);

            string jsonFile = CheckFile(Path.Combine("Files", "concert-data.json"));
            string jsonContent = File.ReadAllText(jsonFile);

            ConcertData concertData =
               JsonConvert.DeserializeObject<ConcertData>(jsonContent);

            PdfReportBuilder ConcertTicketBuilder =
                new PdfReportBuilder();

            ConcertTicketBuilder.TicketData = ticketData;
            ConcertTicketBuilder.ConcertData = concertData;

            var ticketJsonFile1 = CheckFile(
                Path.Combine("Files", "bp-ticket-data.json"));
            string boardingJsonFile = CheckFile(
                Path.Combine("Files", "boarding-data.json"));

            var ticketJsonContent1 = File.ReadAllText(ticketJsonFile1);
            string boardingJsonContent = File.ReadAllText(boardingJsonFile);
            var ticketData1 =
                JsonConvert.DeserializeObject<TicketData1>(ticketJsonContent1);
            BoardingData boardingData =
                JsonConvert.DeserializeObject<BoardingData>(boardingJsonContent);

            string ticketJsonFile0 = CheckFile(Path.Combine("Files", "bp-ticket-data1.json"));
            string ticketJsonContent0 = File.ReadAllText(ticketJsonFile0);
            TicketData1 ticketData0 =
               JsonConvert.DeserializeObject<TicketData1>(ticketJsonContent0);

            string boardingJsonFile0 = CheckFile(
                Path.Combine("Files", "boarding-data1.json"));
            string boardingJsonContent0 = File.ReadAllText(boardingJsonFile0);
            BoardingData boardingData0 =
                JsonConvert.DeserializeObject<BoardingData>(boardingJsonContent0);

            ConcertTicketBuilder.BoardingData = boardingData;
            ConcertTicketBuilder.BoardingData0 = boardingData0;
            ConcertTicketBuilder.TicketData1 = ticketData1;
            ConcertTicketBuilder.TicketData0 = ticketData0;

            return ConcertTicketBuilder.Build(imagePath);
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