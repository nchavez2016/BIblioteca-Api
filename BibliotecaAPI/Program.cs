using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
//using Microsoft.OpenApi.Models;    // ⚠️ SI ESTE NO EXISTE, usa solo el de arriba y Swashbuckle
using Swashbuckle.AspNetCore.SwaggerGen;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Diagnostics;
using StackExchange.Redis;
using BibliotecaAPI.Utilidades.V1; // Para OpenApiSecurityScheme, OpenApiSecurityRequirement


var builder = WebApplication.CreateBuilder(args);

#region Are de Servicios
var origenesPermitidosCORs = builder.Configuration.GetSection("OrigenesPermitidosCORs").Get<String[]>()!;
builder.Services.AddDataProtection(); //agrega proteccion de datos - se puede hacer encritpacion con esta activación

#endregion
//area de servicios
#region Manejo de Cache desde .net mismo
builder.Services.AddOutputCache(opciones =>
        {
        opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
        });

#endregion

#region Manejo de Cache con REDIS
///para usar redis se instala dos paquetes 
/////Microsoft.AspNetCore.OutputCaching.StackExchangeRedis
///Microsoft.Extensions.Caching.StackExchangeRedis

//builder.Services.AddStackExchangeRedisOutputCache(opciones=>
//    {
//        opciones.Configuration = builder.Configuration.GetConnectionString("redis");        
//});

#endregion


//builder.Services.AddAutoMapper(typeof(Program).Assembly);


#region Patron opcion configuracion
//leer configuracion con patron fuertemente tipado
//builder.Services.AddOptions<PersonaOpciones>()
//    .Bind(builder.Configuration.GetSection(PersonaOpciones.Seccion))
//    .ValidateDataAnnotations()
//    .ValidateOnStart();
//builder.Services.AddOptions<TarifaOpciones>()
//    .Bind(builder.Configuration.GetSection(TarifaOpciones.Tarifa))
//    .ValidateDataAnnotations()
//    .ValidateOnStart();

//builder.Services.AddSingleton<PagosProcesamiento>();


#endregion
#region COnfigurar Identity (permisos para el uso del WebAPI)
builder.Services.AddIdentityCore<Usuario>() //IdentityUser clase que representa a usuario
    .AddEntityFrameworkStores<ApplicationDbContext>() // es para que los seriviocs de identiy usen ApplicationDbContext para conectar con tablas de sistema de usuarios
    .AddDefaultTokenProviders();
builder.Services.AddScoped<UserManager<Usuario>>();//configurara servicio que es manejador de usuario (UserManager) y permite registrar usuarios, se pasa la clase de usaurios (IdentityUser)
builder.Services.AddScoped<SignInManager<Usuario>>();////SignInManager permite autentificar usuarios

//builder.Services.AddScoped<MiFiltroDeAccion>();//filtro de accion - EXCLUIDO
builder.Services.AddScoped<FIltroValidacionLibro>();//filtro de accion
builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IServicioAutores, BibliotecaAPI.Servicios.V1.ServicioAutores>();//para optimizar la reutilizacion codigo

builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IGeneradorEnlaces, BibliotecaAPI.Servicios.V1.GeneradorEnlaces>();//para optimizar la reutilizacion codigo
builder.Services.AddScoped<HATEOASAutorAttribute>();
builder.Services.AddScoped<HATEOASAutoresAttribute>();


builder.Services.AddTransient<IServicioUsuario, ServicioUsuario>();//para obtener el id de usario de manera segura
builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();//servicio para almacenar archivo de manera local
builder.Services.AddHttpContextAccessor();//para poder usar en AlmacenadorArchivosLocal la injection IHttpContextAccessor




//builder.Services.AddTransient<IServicioHash, ServicioHash>();//para obtener el id de usario de manera segura




builder.Services.AddHttpContextAccessor();//permite acceder a  contexto http desde cualqueir clase
builder.Services.AddAuthentication().AddJwtBearer(opciones =>  //Bearer es el que tiene algo 
{
    opciones.MapInboundClaims = false; //para que cambie ek claim de uno a otro ya que lo hace autoamtico
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,// no se valida el emisor del token
        ValidateAudience = false,// no se valida audiencia
        ValidateLifetime = true, // tiempo de vida del token
        ValidateIssuerSigningKey = true, // validar llave secreta
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["llaveJWT"]!)),//llave secreta qeu estan firmados los token
        ClockSkew = TimeSpan.Zero //para no tener incovenientes de validacion por discrepancia de tiempo
    };

});

//politica de autentificacion.
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

#region Configuracion Swagger

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "V1",
        Title = "Biblioteca API",
        Description = "Este es un web API para trabajar con datos de autores y libros",

        Contact = new OpenApiContact
        {
            Email = "nch@gmail.com",
            Name = "NCH-Tech"
        },

        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit")
        }
    });
    opciones.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "V2",
        Title = "Biblioteca API",
        Description = "Este es un web API para trabajar con datos de autores y libros",

        Contact = new OpenApiContact
        {
            Email = "nch@gmail.com",
            Name = "NCH-Tech"
        },

        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit")
        }
    });


    // Definimos el esquema JWT Bearer
    opciones.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese solamente el token JWT"
    });



    // Equivalente moderno al Reference = new OpenApiReference(...) /// --> ESTA OPCION ES MANEJADA EN LA CLASE FILTROAUTORIZACION
    opciones.OperationFilter<FiltroAutorizacion>();
    // utilizado en versiones anteriores de Swashbuckle
    //opciones.AddSecurityRequirement(document =>
    //    new OpenApiSecurityRequirement
    //    {
    //        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    //    });
}); 
#endregion



#endregion
#region HAbilida CORSE
builder.Services.AddCors(opciones => {
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        //AllowAnyOrigin -> permitir conectarse cualquier origen
        //WithOrigins->espeifica el origen que puede hacer la llamda
        //AllowAnyMethod-> permitir cualquier metodo
        //AllowAnyHeader->permitir cualquier cabecera

        opcionesCORS.WithOrigins(origenesPermitidosCORs).AllowAnyMethod().AllowAnyHeader()
                    .WithExposedHeaders("cantidad-total-registros");

    });

});
#endregion

// Puede provocar el error según la versión instalada
builder.Services.AddAutoMapper(
    config => { },
    typeof(Program)
);

builder.Services.AddControllers(opciones =>//Filtro global
{
    //opciones.Filters.Add<FiltroTiempoEjecucion>(); EXCLUIDO
    opciones.Conventions.Add(new ConvencionAgrupaPorVersion());

}).AddNewtonsoftJson();



builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseSqlServer("name=DefaultConnection"));


var app = builder.Build();


#region MIDDLEWARES


//are de widdlewares=================================================================================================
#region Manejo de Error
app.UseExceptionHandler(exceptioHandler => exceptioHandler.Run(async context =>
{
        var exceptioHandlerFeture = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptioHandlerFeture?.Error!;

        var error = new Error()
        {
            MensajeDeError = exception.Message,
            StrackTrace = exception.StackTrace,
            Fecha = DateTime.UtcNow
        };
    var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new
    {
        tipo = "Error"
                                        ,
        mensaje = "Ha ocurrido un error inesperado"
                                        ,
        status = 500
    }).ExecuteAsync(context);

})); 
#endregion
app.UseSwagger(); //para poder  servir , contiene una JSON que contiene las rutas del webAP
app.UseSwaggerUI(opciones => //permite visualizar swagger agrupado por version (por pagina)
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API V1");// para usar interfas de usuario para visualiar documen to de swager
    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Biblioteca API V2");// para usar interfas de usuario para visualiar documen to de swager
});


//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Append("mi-cabera", "valor");
//    await next();
//});

app.UseStaticFiles();//para servir a archivos estaticos de wwwroot que es en donde se guraga las imagenes
app.UseCors();

app.UseOutputCache();//necesario para manejara cache 



app.UseAuthentication();
app.UseAuthorization();
//app.Use(async (contexto, next) =>
//{
//    var logger = contexto.RequestServices.GetRequiredService<ILogger<Program>>();
//    logger.LogInformation($"Peticion:{contexto.Request.Method}{contexto.Request.Path}");
//    await next.Invoke();
//    logger.LogInformation($"Respuesta:{contexto.Response.StatusCode}");
//});

//app.UseLogeaticion();
//app.UseBloqueaPeticion();
app.MapControllers();

app.Run();
#endregion
public partial class Program { }
