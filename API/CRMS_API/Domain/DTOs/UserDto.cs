using CRMS_API.Domain.Entities;
namespace CRMS_API.Domain.DTOs
{
    public class UserDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public userRole Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}