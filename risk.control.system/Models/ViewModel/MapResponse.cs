using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace risk.control.system.Models.ViewModel
{
    public class MapResponse
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        public string Bed { get; set; }
        public long? Bath { get; set; }
        public string Size { get; set; }
        public string? Url { get; set; }

        public Position Position
        {
            get; set;
        }
    }

    public class Position
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}