namespace BibliotecaAPI
{

    /*
     Clase para emular un midellware personalizado
     
     */

    public class BloqueaPeticionMidellware
    {
        private readonly RequestDelegate next;

        public BloqueaPeticionMidellware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            if (contexto.Request.Path == "/bloqueado")
            {
                contexto.Response.StatusCode = 403;
                await contexto.Response.WriteAsync("Acceso denegado");
            }            
            await next.Invoke(contexto);
        }

    }
    public static class BloqueaPeticionMidellwareExtension
    {
        public static IApplicationBuilder UseBloqueaPeticion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BloqueaPeticionMidellware>();
        }

    }




}
