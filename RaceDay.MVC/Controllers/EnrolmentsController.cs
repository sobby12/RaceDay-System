
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrolmentsController(IHttpClientFactory httpClientFactory)
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

        // GET: /Enrolments
        // Organizer: View all enrolments.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("UserRole");

            if (!string.Equals(role, "Organizer",
                StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var client = CreateApiClient(token);

            var response = await client.GetAsync("api/Enrolments");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    $"Unable to retrieve enrolments. API response: {(int)response.StatusCode}";

                return View(new List<MyEnrolmentViewModel>());
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<MyEnrolmentViewModel>>();

            return View(enrolments ?? new List<MyEnrolmentViewModel>());
        }

        // GET: Display enrolment form.
        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient(token);

            var eventResponse = await client.GetAsync(
                $"api/Events/{eventId}");

            if (!eventResponse.IsSuccessStatusCode)
                return NotFound();

            var eventItem = await eventResponse.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (eventItem == null)
                return NotFound();

            var categoryResponse = await client.GetAsync(
                $"api/Events/{eventId}/Categories");

            if (!categoryResponse.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Unable to load event categories.";

                return RedirectToAction(
                    "Details", "Events", new { id = eventId });
            }

            var categories = await categoryResponse.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            var model = new EnrolmentViewModel
            {
                EventId = eventId,
                EventName = eventItem.EventName,
                Categories = categories ?? new List<CategoryViewModel>()
            };

            return View(model);
        }

        // POST: Submit participant enrolment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnrolmentViewModel model)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient(token);

            var request = new
            {
                EventId = model.EventId,
                CategoryId = model.CategoryId
            };

            var response = await client.PostAsJsonAsync(
                $"api/Events/{model.EventId}/Enrolments",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    $"Enrolment failed: {(int)response.StatusCode} - {error}";

                return RedirectToAction(
                    "Create", new { eventId = model.EventId });
            }

            TempData["Success"] =
                "You have successfully enrolled in this event!";

            return RedirectToAction("MyEnrolments");
        }

        // GET: Display participant's own enrolments.
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient(token);

            var response = await client.GetAsync(
                "api/Enrolments/Me");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    $"Unable to retrieve enrolments: {(int)response.StatusCode}";

                return View(new List<MyEnrolmentViewModel>());
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<MyEnrolmentViewModel>>();

            return View(
                enrolments ?? new List<MyEnrolmentViewModel>());
        }

        // POST: Cancel participant enrolment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient(token);

            var response = await client.DeleteAsync(
                $"api/Enrolments/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Your enrolment has been successfully cancelled.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] =
                    $"Unable to cancel enrolment: {(int)response.StatusCode} - {error}";
            }

            return RedirectToAction("MyEnrolments");
        }
    }

    public class MyEnrolmentViewModel
    {
        public int EnrollmentId { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
