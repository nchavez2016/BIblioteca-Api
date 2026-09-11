using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class LibroCreacionDTO
    {
      

        [Required]
        [StringLength(250, ErrorMessage = "La longitud del campo {0} debe ser de {1} caracteres")]
        public required string Titulo { get; set; }

        public List<int> AutoresIds { get; set; } = [];


    }
}
