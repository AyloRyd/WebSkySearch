namespace WebSkySearch.Models
{
    public class SearchResult
    {
        public List<Flight>? Flights { get; init; }
        public Airport? OriginAirport { get; init; }
        public Airport? DestinationAirport { get; init; }
    }
}
