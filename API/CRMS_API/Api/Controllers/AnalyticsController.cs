using CRMS_API.Domain.DTOs;
using CRMS_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRMS_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("system-wide")]
        [Authorize(Roles = "SuperAdmin")] // Specifically restricted to SuperAdmins
        public async Task<ActionResult<SystemAnalyticsDto>> GetSystemAnalytics()
        {
            var analytics = await _analyticsService.GetSystemWideAnalyticsAsync();
            return Ok(analytics);
        }

        [HttpGet("owner-overview")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<AdminDashboardDto>> GetOwnerAnalytics()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User identity not found in token." });
            }

            var analytics = await _analyticsService.GetOwnerDashboardAnalyticsAsync(userId);
            return Ok(analytics);
        }
    }
}