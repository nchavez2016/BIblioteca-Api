using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Servicios
{
    public class ServicioLlaves : IServicioLlaves
    {
        private readonly ApplicationDbContext context;

        public ServicioLlaves(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<LlaveAPI> CrearLlave(string usuarioId, TipoLlave tipoLlave)
        {
            var llave = GenerarLlave();

            var llaveApi = new LlaveAPI
            {
                Activa = true,
                Llave = llave,
                TipoLlave = tipoLlave,
                UsuarioId = usuarioId
            };
            context.Add(llaveApi);
            await context.SaveChangesAsync();
            return llaveApi;
        }
        public string GenerarLlave() => Guid.NewGuid().ToString().Replace("-", "");


    }
}
