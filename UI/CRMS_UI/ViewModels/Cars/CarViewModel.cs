namespace CRMS_UI.ViewModels.Cars
{
    public class CarViewModel
    {
        public int Id { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public string OwnerName { get; set; } = string.Empty; 
        public bool IsAvailable { get; set; } = true; 
        public string CurrentStatus => IsAvailable ? "Available" : "On Rent";
    }
}
