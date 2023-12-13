namespace risk.control.system.Models.ViewModel
{
    public class CustomerData
    {
        public string Name { get; set; }
        public double Fare { get; set; }

        public override string ToString()
        {
            return "CustomerData{" +
                    ", Name=" + Name +
                    ", Fare=" + Fare +
                     "}";
        }
    }

    public class BeneficiaryData
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return "BeneficiaryData{" +
                    ", Name=" + Name +
                    ", Value=" + Value +
                     "}";
        }
    }

    public class InvestigationData
    {
        public string Flight { get; set; }
        public string FlightCompany { get; set; }
        public string FlightPlaner { get; set; }
        public DateTime Departure { get; set; }
        public string DepartureAirport { get; set; }
        public string Arrival { get; set; }
        public string ArrivalAirport { get; set; }
        public string Class { get; set; }
        public string ClassAdd { get; set; }
        public string Baggage { get; set; }
        public string BaggageAdd { get; set; }
        public string CheckIn { get; set; }
        public string CheckInAirport { get; set; }

        public override string ToString()
        {
            return "InvestigationData{" +
                    "Flight=" + Flight +
                    ", FlightCompany=" + FlightCompany +
                    ", FlightPlaner=" + FlightPlaner +
                    ", Departure=" + Departure +
                    ", DepartureAirport=" + DepartureAirport +
                    ", Arrival=" + Arrival +
                    ", ArrivalAirport=" + ArrivalAirport +
                    ", Class=" + Class +
                    ", ClassAdd=" + ClassAdd +
                    ", Baggage=" + Baggage +
                    ", BaggageAdd=" + BaggageAdd +
                    ", CheckIn=" + CheckIn +
                    ", CheckInAirport=" + CheckInAirport +
                    "}";
        }
    }

    public class Summarydata
    {
        public string Company { get; set; }
        public string Passenger { get; set; }
        public string Document { get; set; }
        public string TicketNo { get; set; }
        public string Order { get; set; }
        public DateTime Issued { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return "Summarydata{" +
                    "Company=" + Company +
                    ", Passenger=" + Passenger +
                    ", Document=" + Document +
                    ", TicketNo=" + TicketNo +
                    ", Order=" + Order +
                    ", Issued=" + Issued +
                    ", Status=" + Status +
                     "}";
        }
    }
}