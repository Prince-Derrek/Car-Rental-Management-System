using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleResponseDto?> AddVehicleAsync(CreateVehicleDto vehicleDto, int ownerId);
        Task<VehicleResponseDto?> GetVehicleByIdAsync(int vehicleId);
        Task<IEnumerable<VehicleResponseDto>> GetVehiclesByOwnerIdAsync(int ownerId);
        Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync();
        Task<IEnumerable<VehicleResponseDto>> SearchVehiclesAsync(string? make, string? model, int? year, decimal? minPrice, decimal? maxPrice);
        Task<int> GetTotalVehicleCountAsync();
        Task<int> GetAvailableVehicleCountAsync();
        Task<VehicleResponseDto?> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, int ownerId);
        Task<bool> DeleteVehicleAsync(int vehicleId, int ownerId);

        Task<VehicleResponseDto?> ToggleAvailabilityAsync(int vehicleId, int ownerId);

    }
}
