using CRMS_API.Domain.Entities; 
namespace CRMS_API.Domain.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bookingStatus Status { get; set; }

        public string Plate { get; set; }
        public string MakeModel { get; set; }

        public string RenterName { get; set; }
        public decimal TotalPrice { get; set; }

        public int VehicleId { get; set; }
        public int RenterId { get; set; }
    }
}