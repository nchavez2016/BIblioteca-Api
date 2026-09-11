using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilidades
{
    public static class HttpContextExtensions
    {
        public async static Task InsertaParametrosPaginacionEnCabecera<T>(this HttpContext httpcontext
            , IQueryable<T> queryable)
        {
            if (httpcontext is null)
            {
                throw new ArgumentNullException(nameof(httpcontext));
            }
            double cantidad= await queryable.CountAsync();
            httpcontext.Response.Headers.Append("cantidad-total-registros", cantidad.ToString());

        }
    }
}
