using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.DTOs
{
    /// <summary>
    /// DTO para datos de autor sin libros
    /// </summary>
    public class AutorDTO:RecursoDTO
    {

        public int Id { get; set; }
        public required string NombreCompleto { get; set; }

        public string? Foto { get; set; }

    }
}
