
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetToken()
        {
            return HttpContext.Session.GetString("JWToken");
        }

        private HttpClient CreateApiClient(string token)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private bool IsOrganizer()
        {
            return string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Organizer",
                StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Categories
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            var client = CreateApiClient(token);

            var response = await client.GetAsync("api/Categories");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    $"Unable to load categories. API response: {(int)response.StatusCode}";

                return View(new List<CategoryViewModel>());
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            return View(categories ?? new List<CategoryViewModel>());
        }

        // GET: /Categories/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            var client = CreateApiClient(token);

            var response = await client.GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load events.";
                return RedirectToAction(nameof(Index));
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            ViewBag.Events = events ?? new List<EventViewModel>();

            return View(new CategoryViewModel());
        }

        // POST: /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            if (model.EventId <= 0 ||
                string.IsNullOrWhiteSpace(model.CategoryName) ||
                string.IsNullOrWhiteSpace(model.CategoryType))
            {
                ModelState.AddModelError("", "Complete all required fields.");
            }

            if (!ModelState.IsValid)
            {
                var clientForEvents = CreateApiClient(token);
                var eventsResponse = await clientForEvents.GetAsync("api/Events");

                if (eventsResponse.IsSuccessStatusCode)
                {
                    ViewBag.Events = await eventsResponse.Content
                        .ReadFromJsonAsync<List<EventViewModel>>()
                        ?? new List<EventViewModel>();
                }

                return View(model);
            }

            var client = CreateApiClient(token);

            var response = await client.PostAsJsonAsync(
                $"api/Events/{model.EventId}/Categories",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    $"Unable to create category: {(int)response.StatusCode} - {error}";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Categories/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!IsOrganizer())
                return Forbid();

            var client = CreateApiClient(token);

            var response = await client.GetAsync("api/Categories");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            var category = categories?.FirstOrDefault(
                c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: /Categories/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
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
                $"api/Categories/{model.CategoryId}",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    $"Unable to update category: {(int)response.StatusCode} - {error}";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Categories/Delete/5
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
                $"api/Categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Category deleted successfully.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    $"Unable to delete category: {(int)response.StatusCode} - {error}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

