using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<IEnumerable<UserDto>> GetUsersAsync(); // New
        Task<bool> ToggleUserStatusAsync(int userId); // New
    }
}
