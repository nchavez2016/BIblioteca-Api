using AutoMapper;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.Utilidades
{
    public class AutoMapperProfile:Profile
    {
        private object dto;

        public AutoMapperProfile()
        {
            //Autor
            CreateMap<Autor, AutorDTO>().ForMember(dto => dto.NombreCompleto, 
                                        config => config.MapFrom(autor => MapearNombreyApellidoAutor(autor)));


            CreateMap<Autor, AutorConLibrosDTO>().ForMember(dto => dto.NombreCompleto,
                                        config => config.MapFrom(autor => MapearNombreyApellidoAutor(autor)));


            CreateMap<AutorCreacionDTO,Autor>();
            
            CreateMap<AutorCreacionDTOconFoto, Autor>().
                ForMember(ent=>ent.Foto,config=>config.Ignore()); //para ignorar en el mapeo

            CreateMap<Autor, AutorPatchDTO>().ReverseMap();

            CreateMap<AutorLibro, LibroDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.LibroId))
                .ForMember(dto => dto.Titulo, config => config.MapFrom(ent => ent.Libro!.Titulo));
            //creacion de autor con libros
            CreateMap<LibroCreacionDTO, AutorLibro>()
                .ForMember(dto => dto.Libro, config => config.MapFrom(dto=> new Libro { Titulo=dto.Titulo }));

            //Libro
            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroConAutoresDTO>();

            CreateMap<AutorLibro, AutorDTO>()
                    .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AutorId))
                    .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(ent => MapearNombreyApellidoAutor(ent.Autor!)));




            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(ent=>ent.Autores, config=>config.MapFrom(dto=>dto.AutoresIds.Select(id=>new AutorLibro { AutorId=id})));
            //Comentario
            CreateMap<ComentarioCreacionDTO, Comentario>();

            CreateMap<Comentario, ComentarioDTO>()
                    .ForMember(dto => dto.UsuarioEmail, config => config.MapFrom(ent => ent.Usuario!.Email));

            CreateMap<Comentario, ComentarioPatchDTO>().ReverseMap();


            //
            CreateMap<Usuario,UsuarioDTO>();
        }

        private string MapearNombreyApellidoAutor(Autor autor) => $"{autor.Nombres} {autor.Apellidos}";
    }
}
