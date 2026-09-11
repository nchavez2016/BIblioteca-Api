using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BibliotecaAPI.Servicios
{
    public class ServicioUsuario : IServicioUsuario //(se crea interfas para usar  el prinicpio de inyeccion dependencia para depender de objetos abstractos y no objetos conretos )
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IHttpContextAccessor contextAccessor;

        public ServicioUsuario(UserManager<Usuario> userManager, IHttpContextAccessor contextAccessor)
        {
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
        }

        public async Task<Usuario>? ObtenerUsuario()
        {
            var emailClain = contextAccessor.HttpContext!
                            .User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            if (emailClain == null)
            {
                return null;
            }
            var email = emailClain.Value;
            return await userManager.FindByEmailAsync(email);

        }

    }
}
