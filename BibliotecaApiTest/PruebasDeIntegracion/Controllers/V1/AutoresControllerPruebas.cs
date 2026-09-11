using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaApiTest.Utilidades;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace BibliotecaApiTest.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        private static readonly string url = "/api/v1/autores"; //url de mi controlador
        private string nombreBD=Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Get_Devuelve404_CuandoAutorNoExiste()
        {
            //preparación
            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();


            // Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            // Verificación
            var statusCode = respuesta.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: respuesta.StatusCode);

        }

        [TestMethod]
        public async Task Get_DevuelveAutor_CuandoAutorExiste()
        {
            //preparación
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor() { Nombres = "Felipe", Apellidos = "Gavilán" });
            context.Autores.Add(new Autor() { Nombres = "Claudia", Apellidos = "Rodríguez" });
            await context.SaveChangesAsync();

            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();


            // Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            // Verificación
            respuesta.EnsureSuccessStatusCode();

            var autor = JsonSerializer.Deserialize<AutorConLibrosDTO>(
                await respuesta.Content.ReadAsStringAsync(), jsonSerializerOptions)!;

            Assert.AreEqual(expected: 1, autor.Id);

        }
        /// <summary>
        /// verifica qeu el metodo esta protegido
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Post_Devuelve401_CuandoUsuarioNoEstaAutenticado()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);//no se ignora la seguridad - para la prueba

            var cliente = factory.CreateClient();
            var autorCreacionDTO = new AutorCreacionDTO
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: respuesta.StatusCode);
        }

        //se prueba que usuario sea administrador pueda realmente crear un autor

        [TestMethod]
        public async Task Post_Devuelve403_CuandoUsuarioNoEstaAdmino()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);//no se ignora la seguridad - para la prueba
            var token = await CrearUsuario(nombreBD, factory);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


            var autorCreacionDTO = new AutorCreacionDTO
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }


        [TestMethod]
        public async Task Post_Devuelve201_CuandoUsuarioEstaAdmino()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);//no se ignora la seguridad - para la prueba

            var claims = new List<Claim> { adminClaim };


            var token = await CrearUsuario(nombreBD, factory, claims);

            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


            var autorCreacionDTO = new AutorCreacionDTO
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123"
            };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            respuesta.EnsureSuccessStatusCode();


            Assert.AreEqual(expected: HttpStatusCode.Created, actual: respuesta.StatusCode);
        }



    }
}
