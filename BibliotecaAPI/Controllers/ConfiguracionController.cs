using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/configuraciones")]
    public class ConfiguracionController: ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection seccion01;
        private readonly IConfigurationSection seccion02;
        
        private readonly PersonaOpciones _personaOpciones;
        private readonly PagosProcesamiento _pagosProcesamiento;

        public ConfiguracionController(IConfiguration configuration, IOptionsSnapshot<PersonaOpciones> opcionesPersona, PagosProcesamiento pagosProcesamiento)
        {
            this.configuration = configuration;
            seccion01 = configuration.GetSection("seccion_01");
            seccion02 = configuration.GetSection("seccion_02");
            this._personaOpciones=opcionesPersona.Value;
            _pagosProcesamiento= pagosProcesamiento;



        }
        


        [HttpGet("opcionj-monitor")]
        public ActionResult GetTarifas()
        {
            return Ok(_pagosProcesamiento.obtenerTarifas());
        }



        [HttpGet("seccion01_opciones")]
        public ActionResult GetSeccion1Opciones()
        {
            return Ok(_personaOpciones);
        }


        [HttpGet("Todos")]
        public ActionResult GetTodos()
        {
            var hijos = configuration.GetChildren().Select(x => $"{x.Key},{x.Value}");
            return Ok(new { hijos });
        }
        [HttpGet("Proveedor")]
        public ActionResult GetProveedor()
        {
            var proveedor = configuration.GetValue<string>("quien_soy");            
            return Ok(new { proveedor });
        }

        [HttpGet("Seccion01")]
        public ActionResult<string> GetSeccion01()
        {
            var nombre = seccion01.GetValue<string>("nombre");
            var edad = seccion01.GetValue<int>("edad");
            return Ok(new { nombre, edad });
        }
        [HttpGet("Seccion02")]
        public ActionResult<string> GetSeccion02()
        {
            var nombre = seccion02.GetValue<string>("nombre");
            var edad = seccion02.GetValue<int>("edad");
            return Ok(new { nombre, edad });
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var opcion1 = configuration["nombre"];
            var opcion2 = configuration.GetValue<string>("nombre")!;
            return opcion2;
        }


        [HttpGet("Secciones")]
        public ActionResult<string> GetSeccion()
        {
            var opcion1 = configuration["ConnectionStrings:DefaultConnection"];
            var opcion2 = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            var seccion = configuration.GetSection("ConnectionStrings");
            var opcion3 = seccion["DefaultConnection"];

            return opcion3!;
        }

    }
}
