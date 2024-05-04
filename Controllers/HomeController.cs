using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;
using WebSkySearch.Models;

namespace WebSkySearch.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        
        private readonly SearchService _searchService = new(
            "sky-scanner3.p.rapidapi.com", 
            "f96fdeb249msh83c6842a4b02504p158aa3jsn0f1dee97254f"
        );

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
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
