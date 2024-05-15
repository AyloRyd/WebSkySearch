using WebSkySearch.Models;
using System.Text.Json;

namespace WebSkySearch.Services;
public class UserService(IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
{
    private readonly string _usersDirectory = Path.Combine(env.ContentRootPath, "Data", "Users");

    private string GetUserFilePath(string username) => Path.Combine(_usersDirectory, $"{username}.json");

        public void SaveUser(User? user)
        {
            var filePath = GetUserFilePath(user.Username);

            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonSerializer.Serialize(user, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        public User? LoadUser(string username)
        {
            var filePath = GetUserFilePath(username);

            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<User>(json);
        }

        public void UpdateSearchHistory(string originCity, string destinationCity, DateTime flightDate)
        {
            var userJson = httpContextAccessor?.HttpContext?.Session.GetString("User");
            var user = JsonSerializer.Deserialize<User>(userJson);
            if (user == null) return;
            user.SearchHistory ??= [];

            var newSearchRequest = new SearchRequest
            {
                OriginCity = originCity,
                DestinationCity = destinationCity,
                FlightDate = flightDate
            };

            var existingRequest = user.SearchHistory.FirstOrDefault(sr =>
                sr.OriginCity == newSearchRequest.OriginCity &&
                sr.DestinationCity == newSearchRequest.DestinationCity &&
                sr.FlightDate.Date == newSearchRequest.FlightDate.Date);

            if (existingRequest != null)
            {
                if (user.SearchHistory.IndexOf(existingRequest) == 0)
                {
                    return;
                }
                user.SearchHistory.Remove(existingRequest);
                user.SearchHistory.Insert(0, existingRequest);
            }
            else
            {
                user.SearchHistory.Insert(0, newSearchRequest);
            }

            if (user.SearchHistory.Count > 3)
            {
                user.SearchHistory.RemoveAt(user.SearchHistory.Count - 1);
            }

            var updatedUserJson = JsonSerializer.Serialize(user);
            httpContextAccessor?.HttpContext?.Session.SetString("User", updatedUserJson);

            SaveUser(user);
        }

}