namespace BibliotecaAPI.DTOs
{
    /// <summary>
    /// dto para datos Autor con libros, maneja herencia.
    /// </summary>
    public class AutorConLibrosDTO : AutorDTO
    {
     public List<LibroDTO> Libros { get; set; } = new List<LibroDTO>();    
    
    }
}
