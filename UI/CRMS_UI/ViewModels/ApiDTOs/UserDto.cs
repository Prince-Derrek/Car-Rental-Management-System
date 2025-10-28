using CRMS_UI.ViewModels.Auth;

namespace CRMS_UI.ViewModels.ApiDTOs
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
