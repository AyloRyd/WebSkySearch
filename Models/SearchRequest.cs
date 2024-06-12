namespace WebSkySearch.Models;

public class SearchRequest
{
    public string? OriginCity { get; init; }
    public string? DestinationCity { get; init; }
    public DateTime FlightDate { get; init; }
}