
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using System.Diagnostics;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            string? token = HttpContext.Session.GetString("JWToken");
            string? role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.Equals(role, "Organizer",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("OrganizerDashboard");
            }

            if (string.Equals(role, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("ParticipantDashboard");
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult OrganizerDashboard()
        {
            if (!IsLoggedInAs("Organizer"))
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult ParticipantDashboard()
        {
            if (!IsLoggedInAs("Participant"))
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        private bool IsLoggedInAs(string requiredRole)
        {
            string? token = HttpContext.Session.GetString("JWToken");
            string? role = HttpContext.Session.GetString("UserRole");

            return !string.IsNullOrEmpty(token) &&
                   string.Equals(role, requiredRole,
                       StringComparison.OrdinalIgnoreCase);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ??
                            HttpContext.TraceIdentifier
            });
        }
    }
}

