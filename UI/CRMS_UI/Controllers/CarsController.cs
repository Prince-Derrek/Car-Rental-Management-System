using Microsoft.AspNetCore.Mvc;
using CRMS_UI.ViewModels.Cars;
using CRMS_UI.Services.Interfaces;

namespace CRMS_UI.Controllers
{
    public class CarsController : Controller
    {
        private readonly IApiService _apiService;

        public CarsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult CheckAuth(string requiredRole = null)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Auth");
            }
            if (requiredRole != null && HttpContext.Session.GetString("UserRole") != requiredRole)
            {
                return RedirectToAction("UserDashboard", "Dashboard");
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            var userRole = HttpContext.Session.GetString("UserRole");
            var isOwner = userRole == "Owner";

            try
            {
                List<CarViewModel> cars;
                if (isOwner)
                {
                    ViewData["Title"] = "My Fleet Management";
                    cars = await _apiService.GetAsync<List<CarViewModel>>("vehicle/owner", HttpContext);
                }
                else
                {
                    ViewData["Title"] = "Available Fleet for Rent";
                    cars = await _apiService.GetAsync<List<CarViewModel>>("vehicle/all", HttpContext);
                }
                return View(cars);
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Could not fetch vehicles: {ex.Message}";
                return View(new List<CarViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var authCheck = CheckAuth("Owner");
            if (authCheck != null) return authCheck;

            ViewData["Title"] = "Add New Vehicle";
            return View(new CarCreateUpdateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarCreateUpdateViewModel vehicleInput)
        {
            var authCheck = CheckAuth("Owner");
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                return View(vehicleInput);
            }

            try
            {
                await _apiService.PostAsync<CarViewModel, CarCreateUpdateViewModel>("vehicle/add", vehicleInput, HttpContext);
                TempData["SuccessMessage"] = "Vehicle added successfully!";
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to add vehicle: {ex.Message}");
                return View(vehicleInput);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = CheckAuth("Owner");
            if (authCheck != null) return authCheck;

            try
            {
                var car = await _apiService.GetAsync<CarCreateUpdateViewModel>($"vehicle/{id}", HttpContext);
                ViewData["Title"] = $"Edit {car.Make} {car.Model}";
                return View(car);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Vehicle not found or an error occurred.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarCreateUpdateViewModel model)
        {
            var authCheck = CheckAuth("Owner");
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _apiService.PutAsync<CarViewModel, CarCreateUpdateViewModel>($"vehicle/{id}", model, HttpContext);
                TempData["SuccessMessage"] = "Vehicle updated successfully!";
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update vehicle: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var authCheck = CheckAuth("Owner");
            if (authCheck != null) return authCheck;

            try
            {
                var success = await _apiService.DeleteAsync($"vehicle/{id}", HttpContext);
                if (success)
                {
                    TempData["SuccessMessage"] = "Vehicle deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete vehicle. It may have active bookings.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during deletion: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}