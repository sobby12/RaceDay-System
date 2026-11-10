
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResultsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Participants: View own results
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Results/Me");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    $"Unable to retrieve results: {(int)response.StatusCode}";

                return View(new List<MyResultViewModel>());
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<MyResultViewModel>>();

            return View(results ?? new List<MyResultViewModel>());
        }

        // Organizers: View all results
        [HttpGet]
        public async Task<IActionResult> ManageResults()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Results");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    $"Unable to retrieve results: {(int)response.StatusCode}";

                return View(new List<MyResultViewModel>());
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<MyResultViewModel>>();

            return View(results ?? new List<MyResultViewModel>());
        }

        // Organizers: Display record result form
        [HttpGet]
        public IActionResult CreateResult()
        {
            return View(new MyResultViewModel());
        }

        // Organizers: Save a new result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResult(
            MyResultViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (model.EnrollmentId <= 0)
            {
                ViewBag.Error = "Enter a valid Enrolment ID.";
                return View(model);
            }

            if (model.FinishPosition.HasValue &&
                model.FinishPosition.Value <= 0)
            {
                ViewBag.Error = "Finish position must be greater than zero.";
                return View(model);
            }

            if (model.FinishTimeSeconds.HasValue &&
                model.FinishTimeSeconds.Value <= 0)
            {
                ViewBag.Error = "Finish time must be greater than zero.";
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            model.ResultId = 0;

            var response = await client.PostAsJsonAsync(
                $"api/Enrolments/{model.EnrollmentId}/Result",
                model);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();

                ViewBag.Error =
                    $"API Error: {(int)response.StatusCode} - " +
                    $"{response.StatusCode}. Details: {errorDetails}";

                return View(model);
            }

            return RedirectToAction(nameof(ManageResults));
        }
    }

    public class MyResultViewModel
    {
        public int ResultId { get; set; }

        public int EnrollmentId { get; set; }

        public int? FinishPosition { get; set; }

        public int? FinishTimeSeconds { get; set; }

        public string ResultStatus { get; set; } = "Official";
    }
}

