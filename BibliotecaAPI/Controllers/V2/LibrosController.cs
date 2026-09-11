using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/libros")]
    [Authorize(Policy = "esadmin")]//atributo de seguridad con politica configurado- en este caso se aplica para todo el controlador, y se podria poner el atributo solo para metodo especifico
    public class LibrosController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private const string cache = "libros-obtener";
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;

        // private readonly ITimeLimitedDataProtector protectorLimitadiPorTiempo;

        public LibrosController(ApplicationDbContext context
                        , IMapper mapper
                        , IOutputCacheStore outputCacheStore //inyectar el limpiado de cache
                        )
            //, IDataProtectionProvider dataProtectionProvider)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            // protectorLimitadiPorTiempo = dataProtectionProvider.CreateProtector("LibrosController").ToTimeLimitedDataProtector();
        }

        #region Endpoint hablitado por token temporal
        //[HttpGet("listado/otenr-libro")]

        //public ActionResult ObtenerTokenLIstado()
        //{
        //    var textoplanto = Guid.NewGuid().ToString();
        //    var token = protectorLimitadiPorTiempo.Protect(textoplanto, lifetime:TimeSpan.FromSeconds(30));
        //    var url = Url.RouteUrl("OtenerListadoLibrosUsandoToken", new { token }, "https");
        //    return Ok(new { url });
        //}

        ///// <summary>
        ///// metodo que usa token temporal
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("listado/{token}", Name = "OtenerListadoLibrosUsandoToken")] //consultar todos los libros
        //[AllowAnonymous]
        //public async Task<ActionResult> ObtenerListadoUsandoToken(string token)
        //{
        //    try
        //    {
        //        protectorLimitadiPorTiempo.Unprotect(token);
        //    }
        //    catch (Exception)
        //    {

        //        ModelState.AddModelError(nameof(token), "El tiempo a expirado");
        //        return ValidationProblem();
        //    }
        //    var libros = await context.Libros.ToListAsync();
        //    var librosDTO = mapper.Map<IEnumerable<LibroDTO>>(libros);
        //    return Ok(librosDTO);

        //}

        #endregion

        [HttpGet] //consultar todos los libros
        [AllowAnonymous] //permite que un usuaruo no logeado use el metodo de API
        [OutputCache(Tags = [cache])] //para guardar en cache [depende de configuracion en program.cs]
        public async Task<IEnumerable<LibroDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            
            var queryable =  context.Libros.AsQueryable();
            await HttpContext.InsertaParametrosPaginacionEnCabecera(queryable);
            var libros = await queryable
                        .OrderBy(x => x.Titulo)//en paginacion necesario ordenar
                        .Paginar(paginacionDTO)//metodo agregado con extencion
                        .ToListAsync();
                        
            var librosDTO = mapper.Map< IEnumerable<LibroDTO>>(libros);
            return librosDTO;

        }
        /// <summary>
        /// Consulta libros con parametro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}",Name ="ObtenerLibrosV2")]//consultar libro por id
        [AllowAnonymous] //permite que un usuaruo no logeado use el metodo de API
        [OutputCache(Tags = [cache])] //para guardar en cache [depende de configuracion en program.cs]
        public async Task<ActionResult<LibroConAutoresDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(x=>x.Autores)
                    .ThenInclude(x=>x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (libro is null)
            {
                return NotFound();
            }
            var librosDTO = mapper.Map<LibroConAutoresDTO>(libro);

            return librosDTO;
        }
        /// <summary>
        /// Crea libro
        /// </summary>
        /// <param name = "libro" ></ param >
        /// < returns ></ returns >
        [HttpPost]//crear un libro
        [ServiceFilter<FIltroValidacionLibro>()]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            //mapeo
            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor

            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("ObtenerLibrosV2", new { id = libro.Id }, libroDTO);

        }
        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.Autores is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }
        }

        [HttpPut("{id:int}")] //actualizar
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros
                                        .Include(x => x.Autores)
                                        .FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB is null)
            {
                return NotFound();
            }

            libroDB = mapper.Map(libroCreacionDTO, libroDB);
            AsignarOrdenAutores(libroDB);
            
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor

            return NoContent();//204 que significa TODO OK
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id) 
        {

            var registrosBorrados=await context.Libros.Where(x => x.Id==id).ExecuteDeleteAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor
            if (registrosBorrados==0)
            {
                return NotFound();
            }

            return NoContent();//204 que significa TODO OK


        }


    }
}
