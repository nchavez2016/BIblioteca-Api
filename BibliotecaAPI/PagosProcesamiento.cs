using Microsoft.Extensions.Options;

namespace BibliotecaAPI
{
    public class PagosProcesamiento
    {
        private TarifaOpciones _tarfiaOpciones;

        public PagosProcesamiento(IOptionsMonitor<TarifaOpciones> opctionMonitor)
        {

            _tarfiaOpciones = opctionMonitor.CurrentValue;
            opctionMonitor.OnChange(nuevaTarifa =>
            {
                Console.WriteLine("tarfiaOpciones actualizada");
                _tarfiaOpciones = nuevaTarifa;
            });
        }

        public void ProcesarPaAgo()
        {
            //aqui usamos la tarifas
        }

        public TarifaOpciones obtenerTarifas()
        {
            return _tarfiaOpciones;
        }

    }
}
