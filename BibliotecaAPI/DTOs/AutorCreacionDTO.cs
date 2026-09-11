using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class AutorCreacionDTO
    {

        [Required(ErrorMessage = "EL campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "La longitud del campo {0} debe ser de {1} caracteres")]        
        public required string Nombres { get; set; }

        [Required(ErrorMessage = "EL campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "La longitud del campo {0} debe ser de {1} caracteres")]        
        public required string Apellidos { get; set; }

        [StringLength(20, ErrorMessage = "La longitud del campo {0} debe ser de {1} caracteres")]
        public string? Identificacion { get; set; }
        public List<LibroCreacionDTO> Libros { get; set; } = [];

    }
}
