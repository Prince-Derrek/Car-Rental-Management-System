namespace CRMS_UI.ViewModels.Rentals
{
    public enum BookingStatus
    {
        Pending = 1,
        Approved = 2,
        Active = 3,
        Completed = 4,
        Cancelled = 5
    }

    public class RentalViewModel
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string MakeModel { get; set; } = string.Empty;
        public string RenterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();

        public decimal TotalPrice { get; set; }
    }
}
