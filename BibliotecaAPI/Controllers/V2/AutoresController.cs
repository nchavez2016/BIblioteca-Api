using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.ComponentModel;
using System.Linq.Dynamic.Core;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/autores")]
    [Authorize(Policy = "esadmin")]//atributo de seguridad con politica configurado- en este caso se aplica para todo el controlador, y se podria poner el atributo solo para metodo especifico
    //[FiltroAgregarCabeceras("controlador","autores")] EXCLUIDO
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        //private readonly ILogger<AutoresController> logger;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<AutoresController> logger;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServicioAutores servicioAutoresV1;
        private const string contenedor = "autore";

        private const string cache = "autores-obtener";

        //public AutoresController(ApplicationDbContext context, ILogger<AutoresController> logger)
        //{
        //    this.context = context;
        //    this.logger = logger;
        //}
        public AutoresController(ApplicationDbContext context, 
                            IMapper mapper, //para mapear entidades a DTO
                            IAlmacenadorArchivos almacenadorArchivos,
                            ILogger<AutoresController> logger,
                            IOutputCacheStore outputCacheStore, //inyectar el limpiado de cache
                            IServicioAutores servicioAutoresV1  //inyeccion de version de servicio V1
                            )
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
            this.servicioAutoresV1 = servicioAutoresV1;
        }
        /// <summary>
        /// Consultar todos los registros
        /// </summary>
        /// <returns></returns>
        [HttpGet]//api/autor
        [AllowAnonymous] //permite que un usuaruo no logeado use el metodo de API
        [EndpointSummary("Busqueda de autor SIN ID")]
        [EndpointDescription("Obtiene lista de autores y susu libros")]
        [OutputCache(Tags =[cache])] //para guardar en cache [depende de configuracion en program.cs]
        //[ServiceFilter<MiFiltroDeAccion>()] EXCLUIDO
        //[FiltroAgregarCabeceras("accion", "obtener-autores")] EXCLUIDO
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            //    logger.LogTrace("Obteniendo listado de autore");
            //    logger.LogDebug("Obteniendo listado de autore");
            //logger.LogInformation("Obteniendo listado de autore");
            //logger.LogWarning("Obteniendo listado de autore");
            //logger.LogError("Obteniendo listado de autore");
            //logger.LogCritical("Obteniendo listado de autore");
            //throw new NotImplementedException();--codigo para prueba de error

            return await servicioAutoresV1.Get(paginacionDTO);
        }
        /// <summary>
        /// COnsultar registro 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "ObtenerAutorV2")] //api/autores/id
        [AllowAnonymous] //permite que un usuaruo no logeado use el metodo de API
        [EndpointSummary("Busqueda de autor por  ID CON / SIN LIBROS")]
        [EndpointDescription("Otiene lista de autores y su libros, si no encuetra autor retoirna 404")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [OutputCache(Tags = [cache])] //para guardar en cache [depende de configuracion en program.cs]
        public async Task<ActionResult<AutorConLibrosDTO>> Get([Description("El ID del autor")] int id, bool incluirLibros = false)
        {
            var queryable = context.Autores.AsQueryable();
            if (incluirLibros)
            {
                queryable = queryable.Include(x => x.Libros)
                                .ThenInclude(x => x.Libro);
            }
            var autor= await queryable.FirstOrDefaultAsync(x => x.Id == id);
            if (autor is null)
            {
                return NotFound();
            }
            var autorCDTO = mapper.Map<AutorConLibrosDTO>(autor);
            return autorCDTO;
        }

        /// <summary>
        /// Filtrado Dinamico
        /// </summary>
        /// <param name="autorFiltroDTO"></param>
        /// <returns></returns>
        [HttpGet("FiltrarV2")]
        [AllowAnonymous]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {

            var queryable = context.Autores.AsQueryable();

            if (!string.IsNullOrEmpty(autorFiltroDTO.Nombres))
            {
                queryable = queryable.Where(x => x.Nombres.Contains(autorFiltroDTO.Nombres));
            }
            if (!string.IsNullOrEmpty(autorFiltroDTO.Apellidos))
            {
                queryable = queryable.Where(x => x.Apellidos.Contains(autorFiltroDTO.Apellidos));
            }

            if (autorFiltroDTO.IncluirLibro)
            {
                queryable = queryable.Include(x => x.Libros).ThenInclude(x => x.Libro);
            }

            if (autorFiltroDTO.TieneFoto.HasValue)
            {
                if (autorFiltroDTO.TieneFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);
                }
            }

            if (autorFiltroDTO.TieneLibros.HasValue)
            {
                if (autorFiltroDTO.TieneLibros.Value)
                {
                    queryable = queryable.Where(x => x.Libros.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Libros.Any());
                }
            }
            if (!string.IsNullOrEmpty(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x => x.Libros
                                        .Any(y => y.Libro!.Titulo
                                        .Contains(autorFiltroDTO.TituloLibro)));
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdenAscente ? "ascending" : "descending";
                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {

                    queryable = queryable.OrderBy(x => x.Nombres);
                    logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Nombres);
            }

            //------------------------------------------------------
            //ejecuta filtrado
            var autores=await queryable
                              .Paginar(autorFiltroDTO.PaginacionDTO)
                              .ToListAsync();
            if(autorFiltroDTO.IncluirLibro)
            {
                var autoreDTO = mapper.Map<IEnumerable<AutorConLibrosDTO>>(autores);
                return Ok(autoreDTO);

            }
            else
            {
                var autoreDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoreDTO);
            }

           
        }

        /// <summary>
        /// INsertar registro 
        /// </summary>
        /// <param name="autor"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV2", new { id = autor.Id }, autorDTO);
        }


        /// <summary>
        /// INsertar registro 
        /// </summary>
        /// <param name="autor"></param>
        /// <returns></returns>
        [HttpPost("con-fotoV1")]        
        public async Task<ActionResult> PostConFoto([FromForm] 
                                        AutorCreacionDTOconFoto autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            if (autorCreacionDTO.Foto is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, autorCreacionDTO.Foto);
                autor.Foto= url;
            }
            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV2",new {id= autor.Id }, autorDTO); 
        }
        /// <summary>
        /// Actualizar registro
        /// </summary>
        /// <param name="id"></param>
        /// <param name="autor"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Autor>> Put(int id,
                                           [FromForm] AutorCreacionDTOconFoto autorCreacionDTO)//api/autor/id
        {
            var existeAutor=await context.Autores.AnyAsync(x=>x.Id==id);

            if (!existeAutor) return NotFound();                

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            if (autorCreacionDTO.Foto is not null)
            {
                var fotoActual = await context
                                .Autores.Where(x => x.Id == id)
                                 .Select(x => x.Foto).FirstAsync();
                var url = await almacenadorArchivos.Editar(fotoActual,contenedor, autorCreacionDTO.Foto);
                autor.Foto= url;
            }


            context.Update(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor
            return NoContent();//204 que significa TODO OK
        }

        #region PATCH JSON
        [HttpPatch("{id:int}")]
        public  async Task<ActionResult> Patch(int id,JsonPatchDocument<AutorPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }
            var autorDB=await context.Autores.FirstOrDefaultAsync(x=>x.Id==id);
            if (autorDB is null)
            {
                return NotFound();
            }
            var autorPatchDTo=mapper.Map<AutorPatchDTO>(autorDB);
            patchDoc.ApplyTo(autorPatchDTo, ModelState);

            var esValido = TryValidateModel(autorPatchDTo);
            if (!esValido)
            {
                return ValidationProblem();
            }
            mapper.Map(autorPatchDTo, autorDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor
            return NoContent();           


        }

        #endregion



        ///    <summary>
        /// Eliminar registro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Autor>> Delete(int id)
        {

            var autor =await context.Autores.FirstOrDefaultAsync(x=>x.Id== id);//se obtiene el autor
            if (autor is null)
            {
                return NotFound();
            }

            context.Remove(autor); // para remobvr un recurso 
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor
            await almacenadorArchivos.Borrar(autor.Foto, contenedor); //elimina la ruta
                       
            return NoContent();//204 que significa TODO OK

        }

    }
}
