using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI
{

    /*
     Clase para emular un midellware personalizado
     
     */


    public class logeaPeticionPeticionMidellware
    {
        private readonly RequestDelegate next;

        public logeaPeticionPeticionMidellware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            //viene la peticion
            var logger = contexto.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"Peticion:{contexto.Request.Method}{contexto.Request.Path}");

            await next.Invoke(contexto);

            //Se va la respuesta
            logger.LogInformation($"Respuesta:{contexto.Response.StatusCode}");
        }

    }

    public static class LogeaPeticionMidelwareExtension
    {
        public static IApplicationBuilder UseLogeaticion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<logeaPeticionPeticionMidellware>();
        }

    } 


}
