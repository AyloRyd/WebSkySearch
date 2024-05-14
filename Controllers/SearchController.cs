using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSkySearch.Models;
using WebSkySearch.Services;

namespace WebSkySearch.Controllers
{
    public class SearchController(UserService userService, SearchService searchService) : Controller
    {
        private readonly UserService _userService = userService;
        
        private readonly SearchService _searchService = searchService;
        
        [HttpPost]
        public async Task<IActionResult> Results(string originCity, string destinationCity, 
            DateTime flightDate, string nonStop, int stops = 100)
        {
            _userService.UpdateSearchHistory(originCity, destinationCity, flightDate);
            
            if (string.IsNullOrEmpty(nonStop))
            {
                stops = 0;
            }

            var origin = await _searchService.GetAirportsByCityName(originCity);
            var destination = await _searchService.GetAirportsByCityName(destinationCity);

            var originAirport = origin.First();
            var destinationAirport = destination.First();

            var flights = await _searchService.GetFlightsByAirports(originAirport, destinationAirport, flightDate, stops);
                
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
            return Redirect(await _searchService.GetBookingUrl(flight));
        }
    }
}
