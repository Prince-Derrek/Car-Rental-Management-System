﻿namespace CRMS_UI.ViewModels.Auth
{
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Renter"; 
    }
}
