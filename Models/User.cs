using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebSkySearch.Models;

public class User
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? ConfirmPassword { get; set; }

    public List<Flight>? SavedFlights { get; set; }
}