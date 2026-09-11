using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros/{libroId:int}/Comentarios")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "Comentarios-obtener";

        public ComentariosController(ApplicationDbContext context
                                    , IMapper mapper
                                    , IServicioUsuario servicioUsuario
                                    , IOutputCacheStore outputCacheStore //inyectar el limpiado de cache
                                     )
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioUsuario = servicioUsuario;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name ="ObtenerComentariosV1")]
        [AllowAnonymous] //permite que un usuaruo no logeado use el metodo de API
        [EndpointSummary("Busqueda de Comentarios por  ID de libro")]
        [OutputCache(Tags = [cache])] //para guardar en cache [depende de configuracion en program.cs]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(libro => libro.Id == libroId);
            if (!existeLibro) return NotFound();

            var comentarios = await context.Comentarios
                            .Include(x => x.Usuario)
                            .Where(x => x.LibroId == libroId)
                            .OrderByDescending(x => x.FechaPublicacion)
                            .ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{Id}", Name ="ObtenerComentarioV1")]
        [AllowAnonymous] //permite que un usuaruo no logeado use el metodo de API
        [EndpointSummary("Busqueda de Comentarios por  ID de comentario")]
        [OutputCache(Tags = [cache])] //para guardar en cache [depende de configuracion en program.cs]
        public async Task<ActionResult<ComentarioDTO>>  Get(Guid id)
        {
            var comentarios = await context.Comentarios                                
                                    .Include (x => x.Usuario)                                    
                                    .FirstOrDefaultAsync(x => x.Id == id);
            if (comentarios is null) return NotFound();
            Console.WriteLine(comentarios);
            return mapper.Map<ComentarioDTO>(comentarios);
        }

        [HttpPost(Name ="CrearComentarioV1")]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(libro => libro.Id == libroId);
            if (!existeLibro) return NotFound();
            
            var usuario = await servicioUsuario.ObtenerUsuario();            
            if (usuario is null) return NotFound();
            

            var comentario=mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.FechaPublicacion = DateTime.UtcNow;
            comentario.UsuarioId=usuario.Id; //id de usuario que escribio el comentario

            context.Add(comentario);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor

            var comentarioDTO=mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("ObtenerComentarioV1", new { id = comentario.Id, libroId }, comentarioDTO);
        }
        

        [HttpPatch("{id}",Name ="PatchComentarioV1")]
        public async Task<ActionResult> Patch(Guid id,int libroId , JsonPatchDocument<ComentarioPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }
            var existeLibro = await context.Libros.AnyAsync(libro => libro.Id == libroId);
            if (!existeLibro) return NotFound();

            var usuario = await servicioUsuario.ObtenerUsuario();
            if (usuario is null) return NotFound();


            var comentarioDB = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentarioDB is null) return NotFound();

            if (comentarioDB.UsuarioId!=usuario.Id) // pregunta para validar que el comentario pertenece al usuario, caso contrario se responde prohibido
            {
                return Forbid();//403 - prohibio  --- para ev
            }

            var comentarioPatchDTo = mapper.Map<ComentarioPatchDTO>(comentarioDB);

            patchDoc.ApplyTo(comentarioPatchDTo, ModelState);

            var esValido = TryValidateModel(comentarioPatchDTo);
            if (!esValido)
            {
                return ValidationProblem();
            }
            mapper.Map(comentarioPatchDTo, comentarioDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor

            return NoContent();
        }

        [HttpDelete("{id}",Name ="EliminarComentarioV1")]
        public async Task<ActionResult> Delete(Guid id,int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(libro => libro.Id == libroId);
            if (!existeLibro) return NotFound();


            var usuario = await servicioUsuario.ObtenerUsuario();
            if (usuario is null) return NotFound();


            var comentarioDB=await context.Comentarios.FirstOrDefaultAsync(c => c.Id == id);
            if (comentarioDB is null) return NotFound();

            if (comentarioDB.UsuarioId != usuario.Id) // pregunta para validar que el comentario pertenece al usuario, caso contrario se responde prohibido
            {
                return Forbid();//403 - prohibio  --- para ev
            }
            comentarioDB.EstaBorrado = true; //borrado logico

            context.Update(comentarioDB);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);//para limpiar cache cuandose inserta/actualiza un valor

            //var resgistroBorrados = await context.Comentarios.Where(x => x.Id == id).ExecuteDeleteAsync();
            //if (resgistroBorrados==0)
            //{
            //    return NotFound();
            //}
            return NoContent();

        }




    }
}
