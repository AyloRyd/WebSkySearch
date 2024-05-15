using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSkySearch.Models;
using WebSkySearch.Services;

namespace WebSkySearch.Controllers
{
    public class SearchController(UserService userService, SearchService searchService) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Results(string originCity, string destinationCity, 
            DateTime flightDate, string nonStop, int stops = 100)
        {
            if (HttpContext.Session.GetString("User") != null)
            {
                userService.UpdateSearchHistory(originCity, destinationCity, flightDate);
            }

            if (string.IsNullOrEmpty(nonStop))
            {
                stops = 0;
            }

            var origin = await searchService.GetAirportsByCityName(originCity);
            var destination = await searchService.GetAirportsByCityName(destinationCity);

            var originAirport = origin.First();
            var destinationAirport = destination.First();

            var flights = await searchService.GetFlightsByAirports(originAirport, destinationAirport, flightDate, stops);
                
            return View(new SearchResult
            {
                Flights = flights,
                OriginAirport = originAirport,
                DestinationAirport = destinationAirport
            });
        }
        
        [HttpPost]
        public async Task<IActionResult> RedirectToBooking(string flightData)
        {
            var flight = JsonSerializer.Deserialize<Flight>(flightData);
            return Redirect(await searchService.GetBookingUrl(flight));
        }
    }
}
