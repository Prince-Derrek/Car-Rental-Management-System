using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IAnalyticsService
    {
        // System-wide overview for SuperAdmin
        Task<SystemAnalyticsDto> GetSystemWideAnalyticsAsync();

        // Owner-specific overview for standard Admins
        Task<AdminDashboardDto> GetOwnerDashboardAnalyticsAsync(string ownerId);

        // Historical data for trend charts
        Task<DashboardChartDto> GetRevenueTrendAsync();
    }
}