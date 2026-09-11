using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Permissions;

namespace BibliotecaAPI.Swagger
{
    /// <summary>
    /// clase para poner candado a las rutas suiempre y cuando tenga  [Authorize] y no tenga [AllowAnonymous]
    /// </summary>
    public class FiltroAutorizacion : IOperationFilter // para modificar metada que se muestran el pagina Swagger
    {
        public void Apply(
    OpenApiOperation operation,
    OperationFilterContext context)
        {
            // Obtiene los atributos definidos tanto:
            // 1. A nivel del controlador
            // 2. A nivel del método (endpoint)
            //
            // Esto permite detectar [Authorize] aunque esté colocado
            // sobre toda la clase del controlador.
            var atributos = context.MethodInfo
                .DeclaringType!
                .GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true));


            // Verifica si entre los atributos encontrados existe [Authorize].
            // Si existe, significa que el endpoint requiere autenticación.
            var tieneAuthorize = atributos
                .OfType<AuthorizeAttribute>()
                .Any();


            // Verifica si existe [AllowAnonymous].
            // Este atributo indica que el endpoint puede ser utilizado
            // sin autenticación, aunque el controlador tenga [Authorize].
            var tieneAllowAnonymous = atributos
                .OfType<AllowAnonymousAttribute>()
                .Any();


            // Si el endpoint NO tiene [Authorize],
            // no necesitamos agregar seguridad en Swagger.
            //
            // También salimos si tiene [AllowAnonymous],
            // porque explícitamente permite acceso sin autenticación.
            if (!tieneAuthorize || tieneAllowAnonymous)
            {
                return;
            }


            // Creamos una referencia al esquema de seguridad "bearer"
            // que previamente registramos en Program.cs mediante:
            //
            // opciones.AddSecurityDefinition("bearer", ...)
            //
            // En Swashbuckle 10.x / Microsoft.OpenApi 2.x se utiliza
            // OpenApiSecuritySchemeReference en lugar del antiguo:
            //
            // OpenApiSecurityScheme
            // {
            //     Reference = new OpenApiReference(...)
            // }
            var esquema = new OpenApiSecuritySchemeReference(
                "bearer",
                context.Document
            );


            // Indicamos a Swagger que ESTE endpoint requiere
            // el esquema de seguridad Bearer.
            //
            // Como consecuencia, Swagger mostrará el candado 🔒
            // junto al endpoint protegido.
            operation.Security =
            [
                new OpenApiSecurityRequirement
        {
            [esquema] = []
        }
            ];
        }

        //public void Apply(OpenApiOperation operation, OperationFilterContext context)
        //{
        //    if (context.ApiDescription.ActionDescriptor //se ve la descripcion de la accion  
        //        .EndpointMetadata.OfType<AuthorizeAttribute>().Any())//y se ferifica que que tenga un [Authorize] -es decir que el metodo del controlador tenga el atribito [Authorize]
        //    {
        //        return; // se retorna ya que no tiene el atributo y no esta protegido
        //    }
        //    if (context.ApiDescription.ActionDescriptor //se ve la descripcion de la accion  
        //        .EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())//y se ferifica que que tenga un [AllowAnonymous] -es decir que el metodo del controlador tenga el atribito [AllowAnonymous]
        //    {
        //        return; // se retorna ya que no tiene el atributo y no esta protegido
        //    }           

        //    // Swashbuckle 10.x
        //    var esquema = new OpenApiSecuritySchemeReference(
        //        "bearer",
        //        context.Document
        //    );

        //    operation.Security =
        //    [
        //        new OpenApiSecurityRequirement
        //        {
        //            [esquema] = []
        //        }
        //    ];
        //}
    }
}
