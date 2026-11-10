
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetToken()
        {
            return HttpContext.Session.GetString("JWToken");
        }

        private bool IsOrganizer()
        {
            return string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Organizer",
                StringComparison.OrdinalIgnoreCase);
        }

        private HttpClient CreateApiClient(string token)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // GET: Events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient(token);

            var response = await client.GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    $"Unable to load events. API response: {(int)response.StatusCode}";

                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }

        // GET: Events/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient(token);

            var response = await client.GetAsync($"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // GET: Events/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(GetToken()))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            return View(new EventViewModel());
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateApiClient(token);

            var response = await client.PostAsJsonAsync(
                "api/Events", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ViewBag.Error =
                    $"Unable to create event: {(int)response.StatusCode} - {error}";

                return View(model);
            }

            TempData["Success"] = "Event created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            var client = CreateApiClient(token);

            var response = await client.GetAsync($"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var eventItem = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // POST: Events/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventViewModel model)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateApiClient(token);

            var response = await client.PutAsJsonAsync(
                $"api/Events/{model.EventId}", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ViewBag.Error =
                    $"Unable to update event: {(int)response.StatusCode} - {error}";

                return View(model);
            }

            TempData["Success"] = "Event updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Events/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            var client = CreateApiClient(token);

            var response = await client.DeleteAsync(
                $"api/Events/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event deleted successfully.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    $"Unable to delete event: {(int)response.StatusCode} - {error}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

