using CRMS_API.Domain.Data;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Entities;
using CRMS_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMS_API.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;
        public AnalyticsService(AppDbContext context) { _context = context; }

        public async Task<SystemAnalyticsDto> GetSystemWideAnalyticsAsync()
        {
            var now = DateTime.UtcNow;
            var activeBookingStatuses = new[] { bookingStatus.Approved, bookingStatus.Active };

            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalVehicles = await _context.Vehicles.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == bookingStatus.Pending);
            var activeBookings = await _context.Bookings.CountAsync(b => b.Status == bookingStatus.Approved && b.StartDate <= now && b.EndDate >= now);
            var completedBookings = await _context.Bookings.CountAsync(b => b.Status == bookingStatus.Completed);
            var totalRevenue = await _context.Bookings.Where(b => b.Status == bookingStatus.Completed).SumAsync(b => b.TotalPrice);

            var unavailableVehicleIds = await _context.Bookings
                .Where(b => activeBookingStatuses.Contains(b.Status) && b.EndDate >= now) // Include future approved
                .Select(b => b.VehicleId)
                .Distinct()
                .ToListAsync();
            var vehiclesAvailable = totalVehicles - unavailableVehicleIds.Count;


            var mostBookedModelQuery = await _context.Bookings
                .Include(b => b.Vehicle)
                .Where(b => b.Status == bookingStatus.Completed)
                .GroupBy(b => new { b.Vehicle.Make, b.Vehicle.Model })
                .Select(g => new { ModelName = g.Key.Make + " " + g.Key.Model, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            var mostBookedModel = mostBookedModelQuery?.ModelName ?? "N/A";

            var ownerBookingsQuery = await _context.Bookings
                .Include(b => b.Vehicle)
                .ThenInclude(v => v.Owner)
                .Where(b => b.Status == bookingStatus.Completed && b.Vehicle != null && b.Vehicle.Owner != null) // Count completed bookings
                .GroupBy(b => new { b.Vehicle.OwnerId, b.Vehicle.Owner.Name })
                .Select(g => new { OwnerName = g.Key.Name, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            var ownerWithMostBookings = ownerBookingsQuery?.OwnerName ?? "N/A";

            return new SystemAnalyticsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalVehicles = totalVehicles,
                VehiclesAvailable = vehiclesAvailable,
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ActiveBookings = activeBookings,
                CompletedBookings = completedBookings,
                TotalRevenue = totalRevenue,
                MostBookedModel = mostBookedModel,
                OwnerWithMostBookings = ownerWithMostBookings
            };
        }
    }
}