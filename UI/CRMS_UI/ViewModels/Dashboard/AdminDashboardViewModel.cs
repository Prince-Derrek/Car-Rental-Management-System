namespace CRMS_UI.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public string UserName { get; set; } = "Admin";

        public int ActiveRentals { get; set; }

        public int TotalVehicles { get; set; }

        public int PendingApprovals { get; set; }

        public int TrackingEnabledPercent { get; set; }

        public List<string> UtilizationLabels { get; set; } = new() { "Total Fleet", "In Rental", "Awaiting Approval" };
        public List<int> UtilizationValues { get; set; } = new();

        // For the Tracking Gauge
        public List<int> TrackingGaugeData { get; set; } = new(); // [Enabled%, Disabled%]
    }
}