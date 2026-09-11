using BibliotecaAPI.Validaciones;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    /// <summary>
    /// clase de autor 
    /// </summary>
    public class Autor //: IValidatableObject
    {
        public int Id {  get; set; }
        
        [Required(ErrorMessage ="EL campo {0} es requerido")]
        [StringLength(150,ErrorMessage ="La longitud del campo {0} debe ser de {1} caracteres")]
        //[PrimeraLetraMayuscula]
        public required string Nombres {  get; set; }

        [Required(ErrorMessage = "EL campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "La longitud del campo {0} debe ser de {1} caracteres")]
        //[PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }

        [StringLength(20, ErrorMessage = "La longitud del campo {0} debe ser de {1} caracteres")]
        public string? Identificacion { get; set; }
        [Unicode(false)]//para no contemplar cualquier caracter
        public string? Foto { get; set; }
        public List<AutorLibro> Libros { get; set; } = [];

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (!string.IsNullOrEmpty(Nombres))
        //    {
        //        if (Nombres[0].ToString() != Nombres[0].ToString().ToUpper())
        //        {
        //            yield return new ValidationResult("Primera letra debe se mayuscula-por modelo", new string[] { nameof(Nombres) });
        //        }

        //    }

        //}
    }
}
