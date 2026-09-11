using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController()]
    [Route("api/V2/autores-collecion")]
    [Authorize(Policy ="esadmin")]//atributo de seguridad con politica configurado- en este caso se aplica para todo el controlador, y se podria poner el atributo solo para metodo especifico
    public class AutorColeccionController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutorColeccionController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        [HttpGet("{Ids}", Name = "ObtenerAutoresPorIdsV2")]
        public async Task<ActionResult<List<AutorConLibrosDTO>>> Get(string Ids)
        {            
            List<int> idColeccion = Ids
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => int.TryParse(s.Trim(), out int n) ? (int?)n : null)
                                    .Where(n => n.HasValue)
                                    .Select(n => n!.Value)
                                    .ToList();
            if (!idColeccion.Any())
            {
                ModelState.AddModelError(nameof(Ids), "Ningun Id fue encontrado");
                return ValidationProblem();
            }
            var autores = await context.Autores
                            .Include(x=>x.Libros)
                                .ThenInclude(x=>x.Libro)
                            .Where(a=>idColeccion.Contains(a.Id))
                            .ToListAsync();
            if (autores.Count!= idColeccion.Count)
            {
                return NotFound();                     
            }
            var autoreDTO = mapper.Map<List<AutorConLibrosDTO>>(autores);

            return autoreDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(IEnumerable<AutorCreacionDTO> autoresCreacionDTO)
        {
            var autores = mapper.Map<IEnumerable<Autor>>(autoresCreacionDTO);
            context.AddRange(autores);
            await context.SaveChangesAsync();

            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);            
            var idString = string.Join(",", autores.Select(x => x.Id));
            return CreatedAtRoute("ObtenerAutoresPorIdsV2", new { Ids=idString }, autoresDTO);

        }
    }
}
