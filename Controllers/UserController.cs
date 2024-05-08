using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebSkySearch.Models;
using System.Linq;

namespace WebSkySearch.Controllers;

public class UserController : Controller
{
    private const string UsersFilePath = "Data/users.json";

    public IActionResult Login() => View();

    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }

        HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Register(string username, string password)
    {
        var users = LoadUsers();
        if (users.Any(u => u.Username == username))
        {
            ViewBag.ErrorMessage = "Username already exists.";
            return View();
        }

        var newUser = new User { Username = username, Password = password, SavedFlights = new List<Flight>() };
        users.Add(newUser);
        SaveUsers(users);
        HttpContext.Session.SetString("User", JsonSerializer.Serialize(newUser));
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("User");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Delete(string username)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            users.Remove(user);
            SaveUsers(users);
        }
        HttpContext.Session.Remove("User");
        return RedirectToAction("Login");
    }

    private static List<User> LoadUsers()
    {
        if (!System.IO.File.Exists(UsersFilePath))
            return [];

        var json = System.IO.File.ReadAllText(UsersFilePath);
        if (string.IsNullOrWhiteSpace(json)) 
            return [];

        return JsonSerializer.Deserialize<List<User>>(json) ?? [];
    }

    private static void SaveUsers(List<User> users)
    {
        var json = JsonSerializer.Serialize(users);
        System.IO.File.WriteAllText(UsersFilePath, json);
    }

    [HttpPost]
    public IActionResult SaveFlight(string flightData)
    {
        var flight = JsonSerializer.Deserialize<Flight>(flightData);
        var userJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(userJson))
            return Unauthorized();
        
        var user = JsonSerializer.Deserialize<User>(userJson);
        ViewBag.Message = "The flight has already been saved!";
        if (!user.SavedFlights.Any(f => f.Id == flight?.Id))
        {
            user.SavedFlights.Add(flight); 
            ViewBag.Message = "The flight was successfully saved!";
            SaveUsers(LoadUsers().Select(u => u.Username == user.Username ? user : u).ToList()); 
            HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));
        }
        return View();
    }
    
    [HttpPost]
    public IActionResult RemoveFlight(string flightData)
    {
        var flight = JsonSerializer.Deserialize<Flight>(flightData);
        var userJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(userJson))
            return Unauthorized();
        
        var user = JsonSerializer.Deserialize<User>(userJson);
        user?.SavedFlights?.RemoveAll(f => f.Id == flight?.Id);
        SaveUsers(LoadUsers().Select(u => u.Username == user.Username ? user : u).ToList());
        HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));
        return RedirectToAction("SavedFlights");
    }

    public IActionResult SavedFlights()
    {
        var userJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(userJson))
            return Unauthorized();
        
        var user = JsonSerializer.Deserialize<User>(userJson);
        return View(user?.SavedFlights?.OrderBy(f => f.ArrivalTime).ToList());
    }
}