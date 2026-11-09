
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display Login Page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Process Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/login",
                new
                {
                    email = model.Email,
                    password = model.Password
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            var result =
                await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null ||
                string.IsNullOrWhiteSpace(result.Token) ||
                string.IsNullOrWhiteSpace(result.Role))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Login response was invalid.");

                return View(model);
            }

            HttpContext.Session.SetString("JWToken", result.Token);
            HttpContext.Session.SetString("UserRole", result.Role);
            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("FirstName", result.FirstName ?? "");
            HttpContext.Session.SetString("LastName", result.LastName ?? "");
            HttpContext.Session.SetString("UserEmail", result.Email ?? "");

            return RedirectToAction("Index", "Home");
        }

        // Display Registration Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Process Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/register",
                new
                {
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    email = model.Email,
                    phone = model.Phone,
                    password = model.Password
                });

            if (response.IsSuccessStatusCode)
            {
                TempData["RegistrationSuccess"] =
                    "Your account has been created successfully. You can now log in.";

                return RedirectToAction("Login");
            }

            if (response.StatusCode ==
                System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already registered.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please check your details and try again.");
            }

            return View(model);
        }

        // Log Out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }

    // API Login Response
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}

