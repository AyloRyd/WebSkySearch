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
    }

    public class Segment
    {
        public string? Id { get; set; }
        public Airport? OriginAirport { get; set; }
        public Airport? DestinationAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationinMinutes { get; set; }
        public string? Carrier { get; set; }

        public static List<Segment>? GetSegments(JsonElement.ArrayEnumerator enumerator)
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
                    DurationinMinutes = segment.GetProperty("durationInMinutes").GetInt32(),
                    Carrier = segment.GetProperty("marketingCarrier").GetProperty("name").GetString()
                });
            }

            return segments;
        }
    }
}