using CRMS_API.Domain.DTOs;
using CRMS_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMS_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] 
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        public AnalyticsController(IAnalyticsService analyticsService) { _analyticsService = analyticsService; }

        [HttpGet("system-wide")]
        public async Task<ActionResult<SystemAnalyticsDto>> GetSystemAnalytics()
        {
            var analytics = await _analyticsService.GetSystemWideAnalyticsAsync();
            return Ok(analytics);
        }
    }
}