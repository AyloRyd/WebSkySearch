using System.Text.Json;

namespace WebSkySearch.Models
{
    public class Flight
    {
        public string? Id { get; set; }
        public string? Token { get; set; }
        public Airport? OriginAirport { get; set; }
        public Airport? DestinationAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationinMinutes { get; set; }
        public int TimeInDays { get; set; }
        public int StopCount { get; set; }
        public List<string?>? Carriers { get; set; }
        public decimal Price { get; set; }
        public List<Segment>? Segments { get; set; }
        
        public static List<Flight> ParseFlights(JsonDocument doc, int stops)
        {
            List<Flight> flights = [];
            
            var dataElement = doc.RootElement.GetProperty("data").GetProperty("itineraries");
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
                        Segment.ParseSegments(leg.GetProperty("segments").EnumerateArray()) : null
                });
            }

            return flights;
        }

        public static string? ParseBookingUrl(JsonDocument doc, Flight flight)
        {
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

            foreach (var segment in enumerator)
            {
                segments.Add(new Segment
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
                });
            }

            return segments;
        }
    }
}