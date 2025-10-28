using CRMS_UI.Filters;
using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.ApiDTOs; 
using CRMS_UI.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace CRMS_UI.Controllers
{
    [TypeFilter(typeof(AuthorizeRoleFilter), Arguments = new object[] { "SuperAdmin" })] // Secure this
    public class SuperAdminController : Controller
    {
        private readonly IApiService _apiService;
        public SuperAdminController(IApiService apiService) { _apiService = apiService; }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Super Admin Dashboard";
            var viewModel = new SuperAdminDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "Super Admin"
            };

            try
            {
                var analyticsDto = await _apiService.GetAsync<SystemAnalyticsDto>("analytics/system-wide", HttpContext);

                viewModel.TotalUsers = analyticsDto.TotalUsers;
                viewModel.ActiveUsers = analyticsDto.ActiveUsers;
                viewModel.TotalVehicles = analyticsDto.TotalVehicles;
                viewModel.VehiclesAvailable = analyticsDto.VehiclesAvailable;
                viewModel.TotalBookings = analyticsDto.TotalBookings;
                viewModel.PendingBookings = analyticsDto.PendingBookings;
                viewModel.ActiveBookings = analyticsDto.ActiveBookings;
                viewModel.CompletedBookings = analyticsDto.CompletedBookings;
                viewModel.TotalRevenue = analyticsDto.TotalRevenue;
                viewModel.MostBookedModel = analyticsDto.MostBookedModel;
                viewModel.OwnerWithMostBookings = analyticsDto.OwnerWithMostBookings;
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Failed to load dashboard data: {ex.Message}. Check API connection.";
            }
            return View(viewModel);
        }

        public async Task<IActionResult> UserList()
        {
            ViewData["Title"] = "Manage Users";
            List<UserDto> users = new List<UserDto>(); 
            try
            {
                users = await _apiService.GetAsync<List<UserDto>>("auth/users", HttpContext);
            }
            catch (HttpRequestException ex) { TempData["ErrorMessage"] = $"Failed to load users: {ex.Message}"; }
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                await _apiService.PutAsync<object, object>($"auth/users/{id}/toggle-status", null, HttpContext);
                TempData["SuccessMessage"] = "User status updated.";
            }
            catch (HttpRequestException ex) { TempData["ErrorMessage"] = $"Failed to update user status: {ex.Message}"; }
            return RedirectToAction("UserList");
        }


        // Add actions for ForceCancelBooking, ForceDeleteVehicle later
    }
    
}