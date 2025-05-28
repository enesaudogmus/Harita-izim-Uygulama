using System.ComponentModel.DataAnnotations;

namespace DrawingApp.Models
{
    public class DrawingDto
    {
        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "İsim 3 ile 100 karakter arasında olmalıdır.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "GeoJson alanı zorunludur.")]
        public string GeoJson { get; set; } = null!;
    }
}
