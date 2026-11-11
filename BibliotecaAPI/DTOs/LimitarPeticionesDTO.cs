using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class LimitarPeticionesDTO
    {
        public const string seccion = "limitarPeticiones";
        [Required]
        public int PeticionesPorDiaGrautito {  get; set; }

    }
}
