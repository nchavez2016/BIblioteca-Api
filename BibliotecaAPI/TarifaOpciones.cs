using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI
{
    public class TarifaOpciones
    {
        public const string Tarifa = "tarifas";
        [Required]
        public int Dia { get; set; }
        [Required]
        public int Noche { get; set; }


    }
}
