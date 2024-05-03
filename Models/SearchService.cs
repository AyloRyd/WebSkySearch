using System.Text.Json;

namespace WebSkySearch.Models
{
    public class SearchService(string apiHost, string apiKey)
    {
        private static HttpClient Client { get; } = new();
        private readonly string _apiKey = apiKey;
        private readonly string _apiHost = apiHost;

        public async Task<List<Airport>> GetAirportsByCityName(string? cityName)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{_apiHost}/flights/auto-complete?query=" +
                    $"{cityName?.Trim().Replace(" ", "%20")}"),
                Headers =
                {
                    { "X-RapidAPI-Key", _apiKey },
                    { "X-RapidAPI-Host", _apiHost },
                },
            };

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            if (doc.RootElement.GetProperty("status").GetBoolean() == false)
            {
                //string formattedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(body),
                //    Newtonsoft.Json.Formatting.Indented);
                throw new Exception(cityName);
            }
            var dataElement = doc.RootElement.GetProperty("data");

            List<Airport> airports = [];

            foreach (var element in dataElement.EnumerateArray())
            {
                var presentation = element.GetProperty("presentation");

                airports.Add(new Airport
                {
                    Name = presentation.GetProperty("title").GetString(),
                    Code = presentation.GetProperty("suggestionTitle").GetString()?[^5..] != "(Any)" ?
                        element.GetProperty("navigation").GetProperty("relevantFlightParams").GetProperty("skyId").GetString() : "Any",
                    Id = presentation.GetProperty("id").GetString(),
                    City = cityName?.Trim(),
                    Country = presentation.GetProperty("subtitle").GetString(),
                });
            }

            return airports;
        }

        private async Task<string?> GetFlightsByEntityId(string? fromEntityId, string? toEntityId, DateTime date)
        {
            if (fromEntityId is null || toEntityId is null) return null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{_apiHost}/flights/search-one-way?" +
                                     $"fromEntityId={fromEntityId.Replace("=", "%3D")}&" +
                                     $"toEntityId={toEntityId.Replace("=", "%3D")}&" +
                                     $"departDate={date:yyyy-MM-dd}"),
                Headers =
                {
                    { "X-RapidAPI-Key", _apiKey },
                    { "X-RapidAPI-Host", _apiHost },
                },
            };

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();

        }

        public async Task<string?> GetBookingUrl(Flight flight)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{_apiHost}/flights/detail?" +
                                     $"token={flight.Token?.Replace("=", "%3D")}&" +
                                     $"itineraryId={flight.Id}"),
                Headers =
                {
                    { "X-RapidAPI-Key", _apiKey },
                    { "X-RapidAPI-Host", _apiHost },
                },
            };

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            var optionsElement = doc.RootElement.GetProperty("data")
                .GetProperty("itinerary").GetProperty("pricingOptions");

            foreach (var option in optionsElement.EnumerateArray())
            {
                var agent = option.GetProperty("agents").EnumerateArray().First();
                if (agent.GetProperty("name").GetString() == flight.Carriers?[0])
                {
                    return agent.GetProperty("url").GetString();
                }
            }

            return optionsElement.EnumerateArray().First().GetProperty("url").GetString();
        }


        public async Task<List<Flight>?> GetFlightsByAirports(Airport origin, Airport destination, DateTime date, int stops = 0)
        {
            var body = await GetFlightsByEntityId(origin.Id, destination.Id, date);
            if (body is null) return null;

            var doc = JsonDocument.Parse(body);
            var dataElement = doc.RootElement.GetProperty("data").GetProperty("itineraries");

            List<Flight> flights = [];

            foreach (var element in dataElement.EnumerateArray())
            {
                var leg = element.GetProperty("legs").EnumerateArray().First();
                if (leg.GetProperty("stopCount").GetInt32() > stops)
                {
                    continue;
                }

                flights.Add(new Flight
                {
                    Id = element.GetProperty("id").GetString(),
                    Token = doc.RootElement.GetProperty("data").GetProperty("token").GetString(),
                    OriginAirport = new Airport(leg.GetProperty("origin")),
                    DestinationAirport = new Airport(leg.GetProperty("destination")),
                    DepartureTime = leg.GetProperty("departure").GetDateTime(),
                    ArrivalTime = leg.GetProperty("arrival").GetDateTime(),
                    DurationinMinutes = leg.GetProperty("durationInMinutes").GetInt32(),
                    TimeInDays = leg.GetProperty("timeDeltaInDays").GetInt32(),
                    StopCount = leg.GetProperty("stopCount").GetInt32(),
                    Carriers = leg.GetProperty("carriers").GetProperty("marketing").EnumerateArray()
                                    .Select(carrier => carrier.GetProperty("name").GetString()).ToList(),
                    Price = element.GetProperty("price").GetProperty("raw").GetDecimal(),
                    Segments = leg.GetProperty("stopCount").GetInt32() != 0 ?
                        Segment.GetSegments(leg.GetProperty("segments").EnumerateArray()) : null
                });
            }

            return [.. flights.OrderBy(flight => flight.DepartureTime)];
        }
    }
}