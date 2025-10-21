using System.ComponentModel.DataAnnotations;

namespace CRMS_UI.ViewModels.Cars
{
    public class CarCreateUpdateViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Plate must be between 5 and 20 characters.")]
        public string Plate { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1990, 2030)]
        public int? Year { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price per day must be greater than zero.")]
        [Display(Name = "Price Per Day (Ksh)")] // Nice display name for the form
        public decimal? PricePerDay { get; set; }
    }
}
