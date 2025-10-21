using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CRMS_API.Domain.Entities
{
    public enum bookingStatus
    {
        Pending = 1,
        Approved = 2,
        Active = 3,
        Completed = 4,
        Cancelled = 5
    }
    public class Booking
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int RenterId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")] 
        public decimal TotalPrice { get; set; }

        public bookingStatus Status { get; set; }
        public Vehicle Vehicle { get; set; }
        public User Renter { get; set; }
    }
}
