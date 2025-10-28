using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.Entities
{
    public enum userRole
    {
        Renter = 1,
        Owner = 2,
        SuperAdmin = 3
    }
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public userRole Role { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Booking> RentedBookings { get; set; }
        public ICollection<Vehicle> OwnedVehicles { get; set; }
    }
}
