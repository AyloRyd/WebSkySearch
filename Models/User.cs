namespace WebSkySearch.Models;

public class User
{
    public string? Username { get; set; }
    public string? Password { get; set; } 
    public List<Flight>? SavedFlights { get; set; }
}