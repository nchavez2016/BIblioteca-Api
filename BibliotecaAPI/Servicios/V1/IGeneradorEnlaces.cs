using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Servicios.V1
{
    public interface IGeneradorEnlaces
    {
        Task GeneraEnlace(AutorDTO autorDTO);
        Task<ColeccionDeRecursosDTO<AutorDTO>> GeneraEnlaces(List<AutorDTO> autores);
    }
}