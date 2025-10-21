namespace CRMS_API.Domain.DTOs
{
    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Plate { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
    }
}
