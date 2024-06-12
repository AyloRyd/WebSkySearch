using System.Text.Json;
using WebSkySearch.Models;

namespace WebSkySearch.Services
{
    public class SearchService(string? apiHost, string? apiKey)
    {
        private static HttpClient Client { get; } = new();

        public async Task<List<Airport>> GetAirportsByCityName(string? cityName)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{apiHost}/flights/auto-complete?query=" +
                    $"{cityName?.Trim().Replace(" ", "%20")}"),
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", apiHost },
                },
            };

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            if (doc.RootElement.GetProperty("status").GetBoolean() == false)
            {
                throw new Exception(cityName);
            }

            return Airport.ParseAirportsList(doc, cityName);
        }

        private async Task<string?> GetFlightsByEntityId(string? fromEntityId, string? toEntityId, DateTime date)
        {
            if (fromEntityId is null || toEntityId is null) return null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{apiHost}/flights/search-one-way?" +
                                     $"fromEntityId={fromEntityId.Replace("=", "%3D")}&" +
                                     $"toEntityId={toEntityId.Replace("=", "%3D")}&" +
                                     $"departDate={date:yyyy-MM-dd}"),
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", apiHost },
                },
            };

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> GetBookingUrl(FlightData? flightData)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{apiHost}/flights/detail?" +
                                     $"token={flightData?.Token?.Replace("=", "%3D")}&" +
                                     $"itineraryId={flightData?.Id}"),
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", apiHost },
                },
            };

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);

            return Flight.ParseBookingUrl(doc, flightData);
        }

        public async Task<List<Flight>?> GetFlightsByAirports(Airport origin, Airport destination, DateTime date, int stops)
        {
            var body = await GetFlightsByEntityId(origin.Id, destination.Id, date);
            if (body is null) return null;

            var doc = JsonDocument.Parse(body);
            var flights = Flight.ParseFlights(doc, stops);

            return [.. flights.OrderBy(flight => flight.DepartureTime)];
        }
    }
}