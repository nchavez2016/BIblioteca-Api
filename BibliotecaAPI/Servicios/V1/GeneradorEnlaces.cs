using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaAPI.Servicios.V1
{
    public class GeneradorEnlaces : IGeneradorEnlaces
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public GeneradorEnlaces(LinkGenerator linkGenerator, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            this.linkGenerator = linkGenerator;
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task GeneraEnlace(AutorDTO autorDTO)
        {

            var usuario = httpContextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");
            GenerarEnlaces(autorDTO, esAdmin.Succeeded);
        }

        public async Task<ColeccionDeRecursosDTO<AutorDTO>> GeneraEnlaces(List<AutorDTO> autores)
        {
            var resultado = new ColeccionDeRecursosDTO<AutorDTO> { Valores = autores };

            var usuario = httpContextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");

            foreach (var dto in autores)
            {
                GenerarEnlaces(dto, esAdmin.Succeeded);
            }


            resultado.Enlacesa.Add(new DatosHATEOASDTO(
                Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                        "ObtenerAutoresV1", new { })!,
                Descripcion: "Self",
                Metodo: "GET"
                ));
            if (esAdmin.Succeeded)
            {
                //enlace crear autor            
                resultado.Enlacesa.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                            "CrearAutorV1", new { })!,
                    Descripcion: "autor-crear",
                    Metodo: "POST"
                    ));
                //enlace crear autor             con fot
                resultado.Enlacesa.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                            "CrearAutorConFotoV1", new { })!,
                    Descripcion: "autor-crear-con-fot",
                    Metodo: "POST"
                    ));
            }
            return resultado;
        }

        private void GenerarEnlaces(AutorDTO autorDTO, bool esAdmin)
        {
            autorDTO.Enlacesa.Add(
                             new DatosHATEOASDTO(
                                Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                                "ObtenerAutorV1", new { id = autorDTO.Id })!,
                                Descripcion: "self",
                                Metodo: "GET"));
            if (esAdmin)
            {
                autorDTO.Enlacesa.Add(
                           new DatosHATEOASDTO(
                               Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                                "ObtenerAutorV1", new { id = autorDTO.Id })!,
                              Descripcion: "autor-actualiza",
                              Metodo: "PUT"));
                autorDTO.Enlacesa.Add(
                                new DatosHATEOASDTO(
                                    Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                                "ObtenerAutorV1", new { id = autorDTO.Id })!,
                                   Descripcion: "autor-patch",
                                   Metodo: "PATCH"));
                autorDTO.Enlacesa.Add(
                                new DatosHATEOASDTO(
                                    Enlace: linkGenerator.GetPathByRouteValues(httpContextAccessor.HttpContext!,
                                "ObtenerAutorV1", new { id = autorDTO.Id })!,
                                   Descripcion: "autor-borrar",
                                   Metodo: "DELETE"));
            }



        }



    }
}
