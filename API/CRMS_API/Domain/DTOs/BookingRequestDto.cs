using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    public class BookingRequestDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

    }
}
