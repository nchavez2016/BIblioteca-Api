using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BibliotecaApiTest.Utilidades
{
    //para poder probar metodos de un controlador
    public class BasePruebas
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions
            = new JsonSerializerOptions { PropertyNameCaseInsensitive=true};
        protected readonly Claim adminClaim = new Claim("esadmin", "1");

        ///protected >>>>>  para usar los metodos solo desde clases deribadas
        /// <summary>
        /// metodo para contruir ApplicationDbContext de manera mas sencilla
        /// </summary>
        /// <param name="nombreDB"></param>
        /// <returns></returns>
        protected ApplicationDbContext ConstruirContext(string nombreDB)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                              .UseInMemoryDatabase(nombreDB);
            var dbContext = new ApplicationDbContext(opciones.Options);
            return dbContext;
        }

        protected IMapper ConfiguraAutomaper()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // Opcional
            });

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            }, loggerFactory);

            return config.CreateMapper();
        }

        //PARA PRUEBAS DE INTEGRACON- metodo para obtener representacion de web api en memoria para pruebas de integracion
        protected WebApplicationFactory<Program> ConstruirWebApplicationFactory(string nombreBD
                , bool ignorarSeguridad = true //pafa saltar brecha de seguridad
            )
        {
            var factory = new WebApplicationFactory<Program>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor descriptorDBContext = services.SingleOrDefault( // se quieta el proveedor actual de EF para el test, no se puede tener 2 proveedore
                   d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;

                    if (descriptorDBContext is not null) // se quieta el proveedor actual de EF para el test, no se puede tener 2 proveedore
                    {
                        services.Remove(descriptorDBContext);
                    }
                    services.AddDbContext<ApplicationDbContext>(opciones =>
                       opciones.UseInMemoryDatabase(nombreBD));

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(opciones =>//se registra el filtro como filtro global
                        {
                            opciones.Filters.Add(new UsuarioFalsoFiltro());
                        });
                    }

                });

            });
            return factory;
        }
        protected async Task<string> CrearUsuario(string nombreBD, WebApplicationFactory<Program> factory) //prueba para enviar mail por no intera el Claim
           => await CrearUsuario(nombreBD, factory, [], "ejemplo@hotmail.com");

        protected async Task<string> CrearUsuario(string nombreBD, WebApplicationFactory<Program> factory,//prueba que si interesa el Claim pero no interesa el email
            IEnumerable<Claim> claims)
            => await CrearUsuario(nombreBD, factory, claims, "ejemplo@hotmail.com");

        //PARA PRUEBAS DE INTEGRACON- 
        protected async Task<string> CrearUsuario(string nombreBD, WebApplicationFactory<Program> factory,
            IEnumerable<Claim> claims, string email)
        {
            var urlRegistro = "/api/v1/usuarios/registro";
            string token = string.Empty;
            token = await ObtenerToken(email, urlRegistro, factory);

            if (claims.Any())
            {
                var context = ConstruirContext(nombreBD);
                var usuario = await context.Users.Where(x => x.Email == email).FirstAsync();
                Assert.IsNotNull(usuario);

                var userClaims = claims.Select(x => new IdentityUserClaim<string>
                {
                    UserId = usuario.Id,
                    ClaimType = x.Type,
                    ClaimValue = x.Value
                });

                context.UserClaims.AddRange(userClaims);
                await context.SaveChangesAsync();
                var urlLogin = "/api/v1/usuarios/login";
                token = await ObtenerToken(email, urlLogin, factory);
            }

            return token;
        }

        private async Task<string> ObtenerToken(string email, string url,
           WebApplicationFactory<Program> factory)
        {
            var password = "aA123456!";
            var credenciales = new CredencialesUsuarioDTO { Email = email, Password = password };
            var cliente = factory.CreateClient();
            var respuesta = await cliente.PostAsJsonAsync(url, credenciales);
            respuesta.EnsureSuccessStatusCode();

            var contenido = await respuesta.Content.ReadAsStringAsync();
            var respuestaAutenticacion = JsonSerializer.Deserialize<RespuestaAutenticacionDTO>(contenido,
                jsonSerializerOptions)!;

            Assert.IsNotNull(respuestaAutenticacion.Token);

            return respuestaAutenticacion.Token;
        }



    }
}
