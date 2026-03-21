namespace CRMS_API.Domain.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalVehicles { get; set; }
        public int VehiclesAvailable { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public string MostPopularVehicle { get; set; } = "N/A";
        public double OccupancyRate { get; set; }
        public int TrackingEnabledPercent { get; set; }

        // Collections for the Graphical Reports
        public List<int> UtilizationValues { get; set; } = new();
        public List<int> TrackingValues { get; set; } = new();
    }
}