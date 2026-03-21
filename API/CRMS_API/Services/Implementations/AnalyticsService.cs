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
            var sixMonthsAgo = now.AddMonths(-5);
            var activeBookingStatuses = new[] { bookingStatus.Approved, bookingStatus.Active };

            // 1. Summary Statistics
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalVehicles = await _context.Vehicles.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == bookingStatus.Pending);
            var activeBookings = await _context.Bookings.CountAsync(b => b.Status == bookingStatus.Approved && b.StartDate <= now && b.EndDate >= now);
            var completedBookings = await _context.Bookings.CountAsync(b => b.Status == bookingStatus.Completed);
            var totalRevenue = await _context.Bookings.Where(b => b.Status == bookingStatus.Completed).SumAsync(b => b.TotalPrice);

            // 2. Vehicle Availability Logic
            var unavailableVehicleIds = await _context.Bookings
                .Where(b => activeBookingStatuses.Contains(b.Status) && b.EndDate >= now)
                .Select(b => b.VehicleId)
                .Distinct()
                .ToListAsync();
            var vehiclesAvailable = totalVehicles - unavailableVehicleIds.Count;

            // 3. Most Booked Vehicle Model
            var mostBookedModelQuery = await _context.Bookings
                .Include(b => b.Vehicle)
                .Where(b => b.Status == bookingStatus.Completed)
                .GroupBy(b => new { b.Vehicle.Make, b.Vehicle.Model })
                .Select(g => new { ModelName = g.Key.Make + " " + g.Key.Model, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            var mostBookedModel = mostBookedModelQuery?.ModelName ?? "N/A";

            // 4. Top Performing Owner
            var ownerBookingsQuery = await _context.Bookings
                .Include(b => b.Vehicle)
                .ThenInclude(v => v.Owner)
                .Where(b => b.Status == bookingStatus.Completed && b.Vehicle != null && b.Vehicle.Owner != null)
                .GroupBy(b => new { b.Vehicle.OwnerId, b.Vehicle.Owner.Name })
                .Select(g => new { OwnerName = g.Key.Name, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            var ownerWithMostBookings = ownerBookingsQuery?.OwnerName ?? "N/A";

            // 5. GRAPHICAL DATA: Revenue Trends (Last 6 Months)
            var revenueTrendData = await _context.Bookings
                .Where(b => b.Status == bookingStatus.Completed && b.EndDate >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
                .GroupBy(b => new { b.EndDate.Year, b.EndDate.Month })
                .Select(g => new {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Total = g.Sum(b => b.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

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
                OwnerWithMostBookings = ownerWithMostBookings,
                RevenueLabels = revenueTrendData.Select(r => r.Date.ToString("MMM yyyy")).ToList(),
                RevenueValues = revenueTrendData.Select(r => r.Total).ToList(),
                StatusValues = new List<int> { activeBookings, pendingBookings, completedBookings }
            };
        }

        public async Task<AdminDashboardDto> GetOwnerDashboardAnalyticsAsync(string ownerId)
        {
            var now = DateTime.UtcNow;
            var activeStatuses = new[] { bookingStatus.Approved, bookingStatus.Active };

            if (!int.TryParse(ownerId, out int ownerIdInt))
            {
                throw new ArgumentException("Invalid Owner ID format.");
            }

            var totalVehicles = await _context.Vehicles.CountAsync(v => v.OwnerId == ownerIdInt);

            var bookingsQuery = _context.Bookings.Where(b => b.Vehicle.OwnerId == ownerIdInt);

            var totalBookings = await bookingsQuery.CountAsync();
            var pendingBookings = await bookingsQuery.CountAsync(b => b.Status == bookingStatus.Pending);
            var activeBookings = await bookingsQuery.CountAsync(b =>
                activeStatuses.Contains(b.Status) && b.StartDate <= now && b.EndDate >= now);

            var totalRevenue = await bookingsQuery
                .Where(b => b.Status == bookingStatus.Completed)
                .SumAsync(b => b.TotalPrice);

            var unavailableVehicleCount = await bookingsQuery
                .Where(b => activeStatuses.Contains(b.Status) && b.EndDate >= now)
                .Select(b => b.VehicleId)
                .Distinct()
                .CountAsync();

            var vehiclesAvailable = totalVehicles - unavailableVehicleCount;

            var popularVehicle = await bookingsQuery
                .GroupBy(b => new { b.Vehicle.Make, b.Vehicle.Model })
                .Select(g => new { Name = g.Key.Make + " " + g.Key.Model, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            int trackingPercent = 0;

            var trackingEnabledCount = await _context.Vehicles
                   .CountAsync(v => v.OwnerId == ownerIdInt && v.IsTrackingEnabled);

               trackingPercent = totalVehicles > 0 
                   ? (int)((double)trackingEnabledCount / totalVehicles * 100) 
                   : 0;

            return new AdminDashboardDto
            {
                TotalVehicles = totalVehicles,
                VehiclesAvailable = vehiclesAvailable,
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ActiveBookings = activeBookings,
                CompletedBookings = totalBookings, // Or however you define 'Completed'
                TotalRevenue = totalRevenue,
                MostPopularVehicle = popularVehicle?.Name ?? "N/A",
                OccupancyRate = totalVehicles > 0 ? Math.Round((double)activeBookings / totalVehicles * 100, 1) : 0,
                TrackingEnabledPercent = trackingPercent,
                UtilizationValues = new List<int> { totalVehicles, activeBookings, pendingBookings },
                TrackingValues = new List<int> { trackingPercent, 100 - trackingPercent }
            };
        }

        public async Task<DashboardChartDto> GetRevenueTrendAsync()
        {
            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var chart = new DashboardChartDto();

            foreach (var month in lastSixMonths)
            {
                var total = await _context.Bookings
                    .Where(b => b.Status == bookingStatus.Completed &&
                                b.EndDate.Month == month.Month &&
                                b.EndDate.Year == month.Year)
                    .SumAsync(b => b.TotalPrice);

                chart.Labels.Add(month.ToString("MMM yyyy"));
                chart.DataPoints.Add(total);
            }
            return chart;
        }
    }
}