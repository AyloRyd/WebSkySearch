using System.Text.Json;

namespace WebSkySearch.Models
{
    public class Airport
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Id { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        public Airport()
        {
        }

        public Airport(JsonElement json)
        {
            Name = json.GetProperty("name").GetString();
            Code = json.GetProperty("id").GetString();
            Id = json.GetProperty("entityId").GetString();
            City = json.GetProperty("city").GetString();
            Country = json.GetProperty("country").GetString();
        }
    }
}
