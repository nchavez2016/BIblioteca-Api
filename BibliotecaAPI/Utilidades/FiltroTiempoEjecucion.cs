using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroTiempoEjecucion : IAsyncActionFilter
    {
        private readonly ILogger<FiltroTiempoEjecucion> logger;

        public FiltroTiempoEjecucion(ILogger<FiltroTiempoEjecucion> logger )
        {
            this.logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //antes de la ejeción de la acción
            var stopWatch = Stopwatch.StartNew();
            logger.LogInformation($"Inicio Acción:{context.ActionDescriptor.DisplayName }");
            await next();

            //despues
            stopWatch.Stop();
            logger.LogInformation($"Fin acción:{context.ActionDescriptor.DisplayName} - Tiempo {stopWatch.ElapsedMilliseconds} ms");

        }


    }
}
