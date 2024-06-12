using System.Text.Json;

namespace WebSkySearch.Models
{
    public class Flight
    {
        public string? Id { get; init; }
        public string? Token { get; init; }
        public Airport? OriginAirport { get; init; }
        public Airport? DestinationAirport { get; init; }
        public DateTime DepartureTime { get; init; }
        public DateTime ArrivalTime { get; init; }
        public int DurationInMinutes { get; init; }
        public int TimeInDays { get; init; }
        public int StopCount { get; init; }
        public List<string?>? Carriers { get; init; }
        public decimal Price { get; init; }
        public List<Segment>? Segments { get; init; }
        
        public static IEnumerable<Flight> ParseFlights(JsonDocument doc, int stops)
        {
            List<Flight> flights = [];
            
            var dataElement = doc.RootElement.GetProperty("data").GetProperty("itineraries");
            flights.AddRange(from element in dataElement.EnumerateArray()
                let leg = element.GetProperty("legs").EnumerateArray().First()
                where leg.GetProperty("stopCount").GetInt32() <= stops
                select new Flight
                {
                    Id = element.GetProperty("id").GetString(),
                    Token = doc.RootElement.GetProperty("data").GetProperty("token").GetString(),
                    OriginAirport = new Airport(leg.GetProperty("origin")),
                    DestinationAirport = new Airport(leg.GetProperty("destination")),
                    DepartureTime = leg.GetProperty("departure").GetDateTime(),
                    ArrivalTime = leg.GetProperty("arrival").GetDateTime(),
                    DurationInMinutes = leg.GetProperty("durationInMinutes").GetInt32(),
                    TimeInDays = leg.GetProperty("timeDeltaInDays").GetInt32(),
                    StopCount = leg.GetProperty("stopCount").GetInt32(),
                    Carriers = leg.GetProperty("carriers")
                        .GetProperty("marketing")
                        .EnumerateArray()
                        .Select(carrier => carrier.GetProperty("name").GetString())
                        .ToList(),
                    Price = element.GetProperty("price").GetProperty("raw").GetDecimal(),
                    Segments = leg.GetProperty("stopCount").GetInt32() != 0 ? 
                        Segment.ParseSegments(leg.GetProperty("segments").EnumerateArray()) : null
                });

            return flights;
        }

        public static string? ParseBookingUrl(JsonDocument doc, FlightData? flightData)
        {
            var optionsElement = doc.RootElement.GetProperty("data")
                .GetProperty("itinerary").GetProperty("pricingOptions").EnumerateArray();

            string? tripComUrl = null, carrierUrl = null;
            foreach (var option in optionsElement)
            {
                var agent = option.GetProperty("agents").EnumerateArray().First();
                var agentName = agent.GetProperty("name").GetString();

                if (agentName == "Trip.com")
                {
                    tripComUrl = agent.GetProperty("url").GetString();
                    break; 
                }
        
                if (agentName == flightData?.Carriers?[0] && carrierUrl == null)
                {
                    carrierUrl = agent.GetProperty("url").GetString();
                    break;
                }
            }

            return tripComUrl ?? carrierUrl ?? optionsElement.First()
                .GetProperty("agents").EnumerateArray().First()
                .GetProperty("url").GetString();
        }
    }

    public class Segment
    {
        public string? Id { get; set; }
        public Airport? OriginAirport { get; set; }
        public Airport? DestinationAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationInMinutes { get; set; }
        public string? Carrier { get; set; }
        
        public static List<Segment>? ParseSegments(JsonElement.ArrayEnumerator enumerator)
        {
            List<Segment>? segments = [];
            segments.AddRange(enumerator.Select(segment => new Segment
            {
                Id = segment.GetProperty("id").GetString(),
                OriginAirport = new Airport()
                {
                    Name = segment.GetProperty("origin").GetProperty("name").GetString(),
                    Code = segment.GetProperty("origin").GetProperty("displayCode").GetString(),
                    City = segment.GetProperty("origin").GetProperty("parent").GetProperty("name").GetString(),
                    Id = null,
                    Country = segment.GetProperty("origin").GetProperty("country").GetString()
                },
                DestinationAirport = new Airport()
                {
                    Name = segment.GetProperty("destination").GetProperty("name").GetString(),
                    Code = segment.GetProperty("destination").GetProperty("displayCode").GetString(),
                    City = segment.GetProperty("destination").GetProperty("parent").GetProperty("name").GetString(),
                    Id = null,
                    Country = segment.GetProperty("origin").GetProperty("country").GetString()
                },
                DepartureTime = segment.GetProperty("departure").GetDateTime(),
                ArrivalTime = segment.GetProperty("arrival").GetDateTime(),
                DurationInMinutes = segment.GetProperty("durationInMinutes").GetInt32(),
                Carrier = segment.GetProperty("marketingCarrier").GetProperty("name").GetString()
            }));

            return segments;
        }
    }
}