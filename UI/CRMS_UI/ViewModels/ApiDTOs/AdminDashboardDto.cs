namespace CRMS_UI.ViewModels.ApiDTOs
{
    public class AdminDashboardDto
    {
        public int TotalVehicles { get; set; }
        public int ActiveRentals { get; set; }
        public int PendingApprovals { get; set; }
        public int TrackingEnabledPercent { get; set; }

        // These allow the API to send the pre-calculated chart arrays
        public List<int> UtilizationValues { get; set; } = new();
        public List<int> TrackingValues { get; set; } = new();
    }
}