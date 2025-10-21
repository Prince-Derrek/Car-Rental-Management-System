using Microsoft.AspNetCore.Mvc;
using CRMS_UI.ViewModels.Rentals;
using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.Cars;

namespace CRMS_UI.Controllers
{
    public class RentalsController : Controller
    {
        private readonly IApiService _apiService;

        public RentalsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult CheckAuth()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (HttpContext.Session.GetString("UserRole") != "Owner")
            {
                return RedirectToAction("UserDashboard", "Dashboard");
            }

            ViewData["Title"] = "All Rental Bookings";
            try
            {
                var rentals = await _apiService.GetAsync<List<RentalViewModel>>("booking/owner", HttpContext);
                return View(rentals);
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Could not fetch rentals: {ex.Message}";
                return View(new List<RentalViewModel>());
            }
        }

        public async Task<IActionResult> History()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (HttpContext.Session.GetString("UserRole") != "Renter")
            {
                return RedirectToAction("AdminDashboard", "Dashboard");
            }

            ViewData["Title"] = "My Rental History";
            try
            {
                var rentals = await _apiService.GetAsync<List<RentalViewModel>>("booking/mine", HttpContext);
                return View(rentals);
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Could not fetch your history: {ex.Message}";
                return View(new List<RentalViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Rent(int id)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            try
            {
                var car = await _apiService.GetAsync<CarViewModel>($"vehicle/{id}", HttpContext);
                var model = new RentalCreateViewModel { VehicleId = id, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(3) };

                ViewData["CarDetails"] = $"{car.Make} {car.Model} ({car.Plate})";
                ViewData["Title"] = "Book Your Rental";
                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Vehicle not found or unavailable for booking.";
                return RedirectToAction("Index", "Cars");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(RentalCreateViewModel model)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid || model.StartDate >= model.EndDate || model.StartDate < DateTime.Today)
            {
                if (model.StartDate >= model.EndDate)
                    ModelState.AddModelError(string.Empty, "Start Date must be before End Date.");
                if (model.StartDate < DateTime.Today)
                    ModelState.AddModelError(string.Empty, "Start Date cannot be in the past.");

                var car = await _apiService.GetAsync<CarViewModel>($"vehicle/{model.VehicleId}", HttpContext);
                ViewData["CarDetails"] = $"{car.Make} {car.Model} ({car.Plate})";

                return View(model);
            }

            try
            {
                await _apiService.PostAsync<RentalViewModel, RentalCreateViewModel>("booking/booking", model, HttpContext);
                TempData["SuccessMessage"] = "Rental request sent successfully! Awaiting owner approval.";
                return RedirectToAction("History");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Booking failed: {ex.Message}. Check dates and try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, [FromForm] BookingStatus newStatus)
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            if (HttpContext.Session.GetString("UserRole") != "Owner")
            {
                TempData["ErrorMessage"] = "You are not authorized to change booking status.";
                return RedirectToAction("Index");
            }

            try
            {
                var updateModel = new RentalUpdateStatusViewModel { RentalId = id, NewStatus = newStatus };

                var success = await _apiService.PutAsync<bool, RentalUpdateStatusViewModel>($"api/booking/{id}/status", updateModel, HttpContext);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Booking #{id} status successfully changed to {newStatus}.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to update status for booking #{id}. The request was not successful.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Failed to update status: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
