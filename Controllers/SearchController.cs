using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSkySearch.Models;
using WebSkySearch.Services;

namespace WebSkySearch.Controllers
{
    public class SearchController(UserService userService, SearchService searchService) : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Results(string originCity, string destinationCity, DateTime flightDate, int stops = 2)
        {
            if (HttpContext.Session.GetString("User") != null)
            {
                userService.UpdateSearchHistory(originCity, destinationCity, flightDate);
            }

            var origin = await searchService.GetAirportsByCityName(originCity);
            var destination = await searchService.GetAirportsByCityName(destinationCity);

            var originAirport = origin.First();
            var destinationAirport = destination.First();

            var flights = await searchService
                .GetFlightsByAirports(originAirport, destinationAirport, flightDate, stops);
                
            return View(new SearchResult
            {
                Flights = flights,
                OriginAirport = originAirport,
                DestinationAirport = destinationAirport
            });
        }
        
        [HttpGet]
        public async Task<IActionResult> RedirectToBooking(string flightData)
        {
            var decodedFlightData = Uri.UnescapeDataString(flightData);
            var flight = JsonSerializer.Deserialize<FlightData>(decodedFlightData);
            var bookingUrl = await searchService.GetBookingUrl(flight);
            return bookingUrl != null ? Redirect(bookingUrl) : View("Error");
        }
        
        [HttpGet]
        public JsonResult GetCities(string term)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "cities.txt");
            var cities = System.IO.File.ReadAllLines(filePath)
                .Where(city => city.StartsWith(term, StringComparison.OrdinalIgnoreCase)).ToList();

            return Json(cities);
        }
    }
}
