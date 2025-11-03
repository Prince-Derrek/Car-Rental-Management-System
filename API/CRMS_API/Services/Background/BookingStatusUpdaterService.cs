using CRMS_API.Domain.Data;
using CRMS_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMS_API.Services.Background
{
    public class BookingStatusUpdaterService : IHostedService, IDisposable
    {
        private readonly ILogger<BookingStatusUpdaterService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer? _timer;

        public BookingStatusUpdaterService(ILogger<BookingStatusUpdaterService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Status Updater Service is starting.");

            // Set the timer to run the check every 5 minutes.
            // (You can adjust this interval as needed.)
            _timer = new Timer(UpdateBookingStatuses, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private void UpdateBookingStatuses(object? state)
        {
            _logger.LogInformation("Booking Status Updater is running its check.");

            // We must create a new scope to use scoped services like AppDbContext
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.UtcNow;

                // --- 1. Find Bookings to Activate ---
                // Find all 'Approved' bookings where the StartDate has passed
                var bookingsToActivate = context.Bookings
                    .Where(b => b.Status == bookingStatus.Approved && b.StartDate <= now)
                    .ToList();

                if (bookingsToActivate.Any())
                {
                    _logger.LogInformation("Found {Count} bookings to set to Active.", bookingsToActivate.Count);
                    foreach (var booking in bookingsToActivate)
                    {
                        booking.Status = bookingStatus.Active;
                    }
                }

                // --- 2. Find Bookings to Complete ---
                // (Fixing the next logical problem: auto-completing finished rentals)
                // Find all 'Active' bookings where the EndDate has passed
                var bookingsToComplete = context.Bookings
                    .Where(b => b.Status == bookingStatus.Active && b.EndDate <= now)
                    .ToList();

                if (bookingsToComplete.Any())
                {
                    _logger.LogInformation("Found {Count} bookings to set to Completed.", bookingsToComplete.Count);
                    foreach (var booking in bookingsToComplete)
                    {
                        booking.Status = bookingStatus.Completed;
                    }
                }

                // --- 3. Save Changes ---
                if (bookingsToActivate.Any() || bookingsToComplete.Any())
                {
                    try
                    {
                        context.SaveChanges();
                        _logger.LogInformation("Successfully updated booking statuses.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving updated booking statuses to database.");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Status Updater Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0); // Stop the timer
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}