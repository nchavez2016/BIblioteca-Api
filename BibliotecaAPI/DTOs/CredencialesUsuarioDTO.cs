using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class CredencialesUsuarioDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string? Password { get; set; }



    }
}
