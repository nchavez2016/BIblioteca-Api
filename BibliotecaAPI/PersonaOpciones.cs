using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI
{
    public class PersonaOpciones
    {
        public const string Seccion = "seccion_01";
        [Required]
        public required string Nombre { get; set; }
        [Required]
        public int Edad { get; set; }


    }
}
