using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebSkySearch.Models;
using WebSkySearch.Services;

namespace WebSkySearch.Controllers;

public class UserController(UserService userService) : Controller
{
    public IActionResult Login() => View();

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = userService.LoadUser(username);
            if (user?.Password != password)
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            if (userService.LoadUser(username) != null)
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "The password and confirmation password do not match.";
                return View();
            }

            if (password.Length < 7)
            {
                ViewBag.ErrorMessage = "Password must be at least 7 characters long.";
                return View();
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-zA-Z])(?=.*\d).+$"))
            {
                ViewBag.ErrorMessage = "Password must contain both letters and numbers.";
                return View();
            }

            var newUser = new User { Username = username, Password = password, SavedFlights = new List<Flight>() };
            userService.SaveUser(newUser);
            HttpContext.Session.SetString("User", JsonSerializer.Serialize(newUser));
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult SaveFlight(string flightData)
        {
            var flight = JsonSerializer.Deserialize<Flight>(flightData);
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
                return Unauthorized();
            
            var user = JsonSerializer.Deserialize<User>(userJson);
            if (user?.SavedFlights is null)
            {
                return NotFound();
            }
            
            ViewBag.Message = "The flight has already been saved!";
            if (user.SavedFlights.All(f => f.Id != flight?.Id) && flight is not null)
            {
                user.SavedFlights.Add(flight); 
                ViewBag.Message = "The flight was successfully saved!";
                userService.SaveUser(user);
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
            userService.SaveUser(user);
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