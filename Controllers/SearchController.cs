using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;
using WebSkySearch.Models;

namespace WebSkySearch.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchService _searchService = new(
            "sky-scanner3.p.rapidapi.com", 
            "047d4e60edmshde8e081cebe962ep14af10jsn490fb08d99d0"
        );
        
        [HttpPost]
        public async Task<IActionResult> Results(string originCity, string destinationCity, 
            DateTime flightDate, string nonStop, int stops = 100)
        {
            if (string.IsNullOrEmpty(nonStop))
            {
                stops = 0;
            }

            try
            {
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
            catch (Exception ex)
            {
                return View($"{ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> RedirectToBooking(string flightData)
        {
            var flight = JsonSerializer.Deserialize<Flight>(flightData);
            return Redirect(await _searchService.GetBookingUrl(flight));
        }
    }
}
