using System.Text.Json;

namespace WebSkySearch.Models
{
    public class Airport
    {
        public string? Name { get; set; }
        public string? Code { get; init; }
        public string? Id { get; init; }
        public string? City { get; init; }
        public string? Country { get; set; }

        public Airport() { }

        public Airport(JsonElement json)
        {
            Name = json.GetProperty("name").GetString();
            Code = json.GetProperty("id").GetString();
            Id = json.GetProperty("entityId").GetString();
            City = json.GetProperty("city").GetString();
            Country = json.GetProperty("country").GetString();
        }

        public static List<Airport> ParseAirportsList(JsonDocument doc, string? cityName)
        {
            List<Airport> airports = [];
            
            var dataElement = doc.RootElement.GetProperty("data");
            airports.AddRange(from element in dataElement.EnumerateArray()
                let presentation = element.GetProperty("presentation")
                select new Airport
                {
                    Name = presentation.GetProperty("title").GetString(),
                    Code = presentation.GetProperty("suggestionTitle").GetString()?[^5..] != "(Any)"
                        ? element.GetProperty("navigation")
                            .GetProperty("relevantFlightParams")
                            .GetProperty("skyId")
                            .GetString()
                        : "Any",
                    Id = presentation.GetProperty("id").GetString(),
                    City = cityName?.Trim(),
                    Country = presentation.GetProperty("subtitle").GetString(),
                });

            return airports;
        }
    }
}
