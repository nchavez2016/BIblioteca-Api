using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Servicios
{
    public interface IServicioUsuario
    {
        Task<Usuario>? ObtenerUsuario();
    }
}