using CRMS_API.Domain.DTOs;
namespace CRMS_API.Services.Interfaces
{
    public interface IAnalyticsService { Task<SystemAnalyticsDto> GetSystemWideAnalyticsAsync(); }
}