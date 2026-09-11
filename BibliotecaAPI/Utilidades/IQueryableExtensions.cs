using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Utilidades
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T> (this IQueryable<T> queryable,
            PaginacionDTO paginacionDTO)
        {
            return queryable
                .Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecodrsPorPagina)//saltar conjunto de registros 
                .Take(paginacionDTO.RecodrsPorPagina); //tomo los registros 
        }
    }
}
