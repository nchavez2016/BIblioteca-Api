///metodo para paginacion con extrencion de Iquery
namespace BibliotecaAPI.DTOs
{
    
    public record PaginacionDTO(int Pagina=1, int RecodrsPorPagina=10)
    {
        public const int CantidadMaximaRecordsPorPAgina = 50;
        public int Pagina { get; init; } = Math.Max(1, Pagina);//

        public int RecodrsPorPagina { get; init; } = Math.Clamp(RecodrsPorPagina,1, CantidadMaximaRecordsPorPAgina);//


    }
}
