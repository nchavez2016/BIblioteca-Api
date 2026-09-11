using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilidades
{
    public class FIltroValidacionLibro : IAsyncActionFilter
    {
        private readonly ApplicationDbContext dbContext;

        public FIltroValidacionLibro(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            if (!context.ActionArguments.TryGetValue("LibroCreacionDTO",out var value) 
                        || value is not LibroCreacionDTO libroCreacionDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es valido");
                context.Result = context.ModelState.ContruirProblemDetail();
                return;
            }

            //se valida que exista el libro venga con la info de autor para la creacion de libro
            if (libroCreacionDTO.AutoresIds == null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), $"No se pueden crear libro sin autores");
                context.Result = context.ModelState.ContruirProblemDetail();
                return;
            }
            //se valida que el autor exista en la bbdd
            var autoresIdExisten = await dbContext.Autores.Where(x => libroCreacionDTO.AutoresIds
                                                        .Contains(x.Id))
                                                        .Select(x => x.Id)
                                                        .ToListAsync();
            //se valida que todos los autores enviados en la data exitan en la base de datos (deben ser iguales)
            if (autoresIdExisten.Count != libroCreacionDTO.AutoresIds.Count)
            {
                //se identifica autores que no existen
                var autoresNoExisten = libroCreacionDTO.AutoresIds.Except(autoresIdExisten);
                var autoresNoExistenString = string.Join(",", autoresNoExisten);
                var mensageDeError = $"Los siguientes autores no existen:{autoresNoExistenString}";
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensageDeError);
                context.Result = context.ModelState.ContruirProblemDetail();
                return;
            }


            await next();
        }
    }
}
