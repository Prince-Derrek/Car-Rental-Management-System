using CRMS_API.Domain.Data;
using CRMS_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMS_API.Services.Background
{
    public class BookingStatusUpdaterService : BackgroundService
    {
        private readonly ILogger<BookingStatusUpdaterService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        // Check every 5 minutes
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public BookingStatusUpdaterService(ILogger<BookingStatusUpdaterService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Status Updater Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating booking statuses.");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task UpdateStatusesAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.UtcNow;

                // 1. Activate Bookings: Approved (2) -> Active (3)
                // We only activate if the start date has passed
                var bookingsToActivate = await context.Bookings
                    .Where(b => b.Status == bookingStatus.Approved && b.StartDate <= now)
                    .ToListAsync();

                foreach (var booking in bookingsToActivate)
                {
                    booking.Status = bookingStatus.Active;
                }

                // 2. Complete Bookings: Active (3) -> Completed (4)
                // We only complete if the end date has passed
                var bookingsToComplete = await context.Bookings
                    .Where(b => b.Status == bookingStatus.Active && b.EndDate <= now)
                    .ToListAsync();

                foreach (var booking in bookingsToComplete)
                {
                    booking.Status = bookingStatus.Completed;
                }

                // 3. Save if changes were made
                if (bookingsToActivate.Any() || bookingsToComplete.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Background Service: {Act} bookings activated, {Comp} bookings completed at {Time}",
                        bookingsToActivate.Count, bookingsToComplete.Count, now);
                }
            }
        }
    }
}