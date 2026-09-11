using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace BibliotecaAPI.Servicios
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// cpntructor con injection de dependencia
        /// </summary>
        /// <param name="env"></param>
        /// <param name="httpContextAccessor"></param>
        public AlmacenadorArchivosLocal(IWebHostEnvironment env //para acceder a una caspeta en el servidor delas imagenes
            ,IHttpContextAccessor httpContextAccessor )//es para acceder al contexto http
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Almacenar(string contenedor, IFormFile archivo)
        {
            var extencion = Path.GetExtension(archivo.FileName);
            var nombrearchivo = $"{Guid.NewGuid()}{extencion}";

            string folder=Path.Combine(env.WebRootPath, contenedor);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string ruta = Path.Combine(folder,nombrearchivo);
            
            using (var ms=new MemoryStream())
            {
                await archivo.CopyToAsync(ms);
                var contenido = ms.ToArray();
                await File.WriteAllBytesAsync(ruta, contenido);
            }
            var url = $"{httpContextAccessor.HttpContext!.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
            var urlArchivo = Path.Combine(url,contenedor,nombrearchivo).Replace("\\","/");

            return urlArchivo;

        }



        public Task Borrar(string? ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return Task.CompletedTask;
            }
            var nombreArchivo=Path.GetFileName(ruta);
            var direcorioArchivo= Path.Combine(env.WebRootPath,contenedor, nombreArchivo);

            if (File.Exists(direcorioArchivo))
            {
                File.Delete(direcorioArchivo);
            }
            return Task.CompletedTask;
        }
    }
}
