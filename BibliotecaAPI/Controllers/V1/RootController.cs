using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RootController: ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name ="ObtenerRootV1")]
        [AllowAnonymous]
        public async Task<IEnumerable<DatosHATEOASDTO>> Get()
        {
            var datosHATEAOS = new List<DatosHATEOASDTO>();

            var esAdmin = authorizationService.AuthorizeAsync(User, "esadmin");
           
            //acciones que cualquiera puede realizar
            datosHATEAOS.Add(new DatosHATEOASDTO(
                                Enlace: Url.Link("ObtenerRootV1", new { })!
                                , Descripcion: "self", Metodo: "GET")
                               );
            datosHATEAOS.Add(new DatosHATEOASDTO(
                             Enlace: Url.Link("ObtenerAutoresV1", new { })!
                             , Descripcion: "autores-obtener", Metodo: "GET")
                            );
            datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerRootV1", new { })!,
                            Descripcion: "self", Metodo: "GET"));

            datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerAutoresV1", new { })!,
                Descripcion: "autores-obtener", Metodo: "GET"));

          

            datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerUsuariosV1", new { })!,
                Descripcion: "usuarios-obtener", Metodo: "GET"));

            datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("RegistroUsuarioV1", new { })!,
                Descripcion: "usuario-registrar", Metodo: "POST"));

            datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("LoginUsuarioV1", new { })!,
                Descripcion: "usuario-login", Metodo: "POST"));

           
            if (User.Identity!.IsAuthenticated)
            {
                //acciones para usuarios logeados
                datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ActualizarUsuarioV1", new { })!,
               Descripcion: "usuario-actualizar", Metodo: "PUT"));

                datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("RenovarTokenV1", new { })!,
                    Descripcion: "token-renovar", Metodo: "GET"));
            }

            if (esAdmin.IsCompletedSuccessfully)
            {
            //acciones que solo ADMIN puede realizar
                datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorV1", new { })!,
                              Descripcion: "autor-crear", Metodo: "POST"));

                datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutoresV1", new { })!,
                    Descripcion: "autores-crear", Metodo: "POST"));

                datosHATEAOS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearLibroV1", new { })!,
                    Descripcion: "libro-crear", Metodo: "POST"));
            }


            return datosHATEAOS;

        }



    }
}
