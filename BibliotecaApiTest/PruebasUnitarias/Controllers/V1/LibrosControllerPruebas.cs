using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaApiTest.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotecaApiTest.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class LibrosControllerPruebas : BasePruebas
    {
        [TestMethod]
        public async Task Get_RetornarCeroLibros_CuandoNoHayLibros()
        {
            // Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfiguraAutomaper();
            IOutputCacheStore outputCacheStore = null!;

            var controller = new LibrosController(context, mapper, outputCacheStore);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();// para pruebas con metodo que utiliza http context 

            var paginacionDTO = new PaginacionDTO(1, 1);

            // Prueba

            var respuesta = await controller.Get(paginacionDTO);

            // Verificación
            Assert.AreEqual(expected: 0, actual: respuesta.Count());
        }
    }
}
