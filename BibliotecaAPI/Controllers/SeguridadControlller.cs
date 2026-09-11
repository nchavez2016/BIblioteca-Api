using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/seguridad")]
    public class SeguridadControlller: ControllerBase
    {
        private readonly IDataProtector protector;
        private readonly ITimeLimitedDataProtector protectorLImitadoPorTiempo;
        private readonly IServicioHash servicioHash;

        public SeguridadControlller(IDataProtectionProvider dataProtectionProvider
            , IServicioHash servicioHash)
        {
            protector = dataProtectionProvider.CreateProtector("SeguridadControlller");
            protectorLImitadoPorTiempo = protector.ToTimeLimitedDataProtector();
            this.servicioHash = servicioHash;
        }
        /// <summary>
        /// metodo para prueba de creacion de hash
        /// </summary>
        /// <param name="textoPlano"></param>
        /// <returns></returns>
        [HttpGet("hash")]
        public ActionResult Hash(string textoPlano)
        {
            //se generan 3 para validar pruebas
            var hash1=servicioHash.Hash(textoPlano); //prueba uno
            var hash2 = servicioHash.Hash(textoPlano); //se envia de nuevo para probar que se genera hash distintos
            var hash3 = servicioHash.Hash(textoPlano, hash2.Sal); //se usa la sal de has2 par prueba
            var resultado = new {textoPlano,hash1,hash2,hash3};
            return Ok(resultado);
        }


        [HttpGet("encriptar-limitadoportiempo")]
        public ActionResult EncriptarLimitadoPorTiempo(string textoPlano)
        {
            string textoCifrado = protectorLImitadoPorTiempo.Protect(textoPlano, lifetime:TimeSpan.FromSeconds(30));
            return Ok(new { textoCifrado });

        }
        [HttpGet("desencriptar-limitadoportiempo")]
        public ActionResult DesencriptarLimitadoPorTiempo(string textoCifrado)
        {
            string textoPlano = protectorLImitadoPorTiempo.Unprotect(textoCifrado);
            return Ok(new { textoPlano });

        }

        [HttpGet("encriptar")]
        public ActionResult Encriptar(string textoPlano)
        {
            string textoCifrado = protector.Protect(textoPlano);
            return Ok(new { textoCifrado });

        }
        [HttpGet("desencriptar")]
        public ActionResult Desencriptar(string textoCifrado)
        {
            string textoPlano = protector.Unprotect(textoCifrado);
            return Ok(new { textoPlano });

        }

    }
}
