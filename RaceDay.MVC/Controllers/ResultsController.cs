
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

