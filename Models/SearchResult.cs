namespace WebSkySearch.Models
{
    public class SearchResult
    {
        public List<Flight>? Flights { get; set; }
        public Airport? OriginAirport { get; set; }
        public Airport? DestinationAirport { get; set; }
    }
}
