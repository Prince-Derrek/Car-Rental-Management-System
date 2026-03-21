namespace CRMS_UI.ViewModels.ApiDTOs
{
    public class SystemAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalVehicles { get; set; }
        public int VehiclesAvailable { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public string MostBookedModel { get; set; }
        public string OwnerWithMostBookings { get; set; }

        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();

        public List<string> StatusLabels { get; set; } = new() { "Active", "Pending", "Completed" };
        public List<int> StatusValues { get; set; } = new();
    }
}