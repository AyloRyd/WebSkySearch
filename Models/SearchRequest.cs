namespace WebSkySearch.Models;

public class SearchRequest
{
    public string? OriginCity { get; set; }
    public string? DestinationCity { get; set; }
    public DateTime FlightDate { get; set; }
}