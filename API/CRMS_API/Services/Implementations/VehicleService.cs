using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Data;
using Microsoft.EntityFrameworkCore;
using CRMS_API.Domain.Entities;

namespace CRMS_API.Services.Implementations
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleResponseDto?> AddVehicleAsync(CreateVehicleDto vehicleDto, int ownerId)
        {
            var owner = await _context.Users.FindAsync(ownerId);
            if (owner == null)
            {
                return null;
            }

            var newVehicle = new Vehicle
            {
                OwnerId = ownerId,
                Make = vehicleDto.Make,
                Model = vehicleDto.Model,
                Plate = vehicleDto.Plate,
                Year = vehicleDto.Year,
                PricePerDay = vehicleDto.PricePerDay
            };

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            return new VehicleResponseDto
            {
                Id = newVehicle.Id,
                Make = newVehicle.Make,
                Model = newVehicle.Model,
                Plate = newVehicle.Plate,
                Year = newVehicle.Year,
                OwnerId = newVehicle.OwnerId,
                OwnerName = owner.Name,
                PricePerDay = vehicleDto.PricePerDay
            };
        }

        public async Task<IEnumerable<VehicleResponseDto>> SearchVehiclesAsync(string? make, string? model, int? year, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Vehicles.Include(v => v.Owner).AsQueryable();

            // Dynamic Filtering
            if (!string.IsNullOrWhiteSpace(make))
                query = query.Where(v => v.Make.Contains(make));

            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(v => v.Model.Contains(model));

            if (year.HasValue)
                query = query.Where(v => v.Year == year.Value);

            if (minPrice.HasValue)
                query = query.Where(v => v.PricePerDay >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(v => v.PricePerDay <= maxPrice.Value);

            return await query.Select(v => new VehicleResponseDto
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Plate = v.Plate,
                Year = v.Year,
                PricePerDay = v.PricePerDay, // Ensure Price is returned
                OwnerId = v.OwnerId,
                OwnerName = v.Owner.Name
            }).ToListAsync();
        }
        public async Task<VehicleResponseDto?> GetVehicleByIdAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            return vehicle == null ? null : MapVehicleToResponseDto(vehicle);
        }
        public async Task<IEnumerable<VehicleResponseDto>> GetVehiclesByOwnerIdAsync(int ownerId)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.OwnerId == ownerId)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Plate = v.Plate,
                    Year = v.Year,
                    OwnerId = v.OwnerId,
                    OwnerName = v.Owner.Name
                })
                .ToListAsync();
            return vehicles;
        }
        public async Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Owner)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Plate = v.Plate,
                    Year = v.Year,
                    PricePerDay = v.PricePerDay,
                    OwnerId = v.OwnerId,
                    OwnerName = v.Owner.Name
                }).ToListAsync();
            return vehicles;
        }
        public async Task<int> GetTotalVehicleCountAsync()
        {
            return await _context.Vehicles.CountAsync();
        }

        public async Task<int> GetAvailableVehicleCountAsync()
        {
            var unavailableVehicleIds = await _context.Bookings
                .Where(b => b.Status == bookingStatus.Active)
                .Select(b => b.VehicleId)
                .Distinct()
                .ToListAsync();

            return await _context.Vehicles
                .CountAsync(v => !unavailableVehicleIds.Contains(v.Id));
        }
        public async Task<VehicleResponseDto?> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, int ownerId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null || vehicle.OwnerId != ownerId)
            {
                return null;
            }

            vehicle.Make = vehicleDto.Make;
            vehicle.Model = vehicleDto.Model;
            vehicle.Year = vehicleDto.Year;

            await _context.SaveChangesAsync();

            return MapVehicleToResponseDto(vehicle);
        }
        public async Task<bool> DeleteVehicleAsync(int vehicleId, int ownerId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null || vehicle.OwnerId != ownerId)
            {
                return false;
            }

            bool hasActiveOrApprovedBookings = vehicle.Bookings.Any(b =>
                    b.Status == bookingStatus.Active ||
                    b.Status == bookingStatus.Approved);

            if (hasActiveOrApprovedBookings)
            {
                return false;
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }
        private VehicleResponseDto MapVehicleToResponseDto(Vehicle vehicle)
        {
            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                Plate = vehicle.Plate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PricePerDay = vehicle.PricePerDay,
                OwnerName = vehicle.Owner?.Name ?? "N/A"
            };
        }
    }
}
