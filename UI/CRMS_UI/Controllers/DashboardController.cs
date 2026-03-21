using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.ApiDTOs;
using CRMS_UI.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
namespace CRMS_UI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;

        public DashboardController(IApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult CheckAuthentication()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (HttpContext.Session.GetString("UserRole") != "Owner")
            {
                return RedirectToAction("UserDashboard");
            }

            var viewModel = new AdminDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "Admin"
            };

            try
            {
                // 1. Single API call to get the entire owner-specific dataset
                var dto = await _apiService.GetAsync<AdminDashboardDto>("analytics/owner-overview", HttpContext);

                // 2. Map Summary Stats
                viewModel.ActiveRentals = dto.ActiveRentals;
                viewModel.TotalVehicles = dto.TotalVehicles;
                viewModel.PendingApprovals = dto.PendingApprovals;
                viewModel.TrackingEnabledPercent = dto.TrackingEnabledPercent;

                // 3. Map Chart Data
                viewModel.UtilizationValues = new List<int> { dto.TotalVehicles, dto.ActiveRentals, dto.PendingApprovals };
                viewModel.TrackingGaugeData = new List<int> { dto.TrackingEnabledPercent, 100 - dto.TrackingEnabledPercent };
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Dashboard error: {ex.Message}";
            }

            return View(viewModel);
        }

        public async Task<IActionResult> UserDashboard()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            ViewData["Title"] = "User Dashboard";

            var viewModel = new UserDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "Renter"
            };

            try
            {
                var availableCarsTask = _apiService.GetAsync<int>("vehicle/count/available", HttpContext);
                var activeBookingsTask = _apiService.GetAsync<int>("booking/count/my-active", HttpContext);
                var pendingApprovalsTask = _apiService.GetAsync<int>("booking/count/my-pending", HttpContext);

                await Task.WhenAll(availableCarsTask, activeBookingsTask, pendingApprovalsTask);

                viewModel.AvailableCarsCount = availableCarsTask.Result;
                viewModel.ActiveBookingsCount = activeBookingsTask.Result;
                viewModel.PendingApprovalsCount = pendingApprovalsTask.Result;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to fetch your dashboard data: {ex.Message}";
            }

            return View(viewModel);
        }
    }
}