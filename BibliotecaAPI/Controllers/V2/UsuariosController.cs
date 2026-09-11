using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServicioUsuario servicioUsuario;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        /// <summary>
        /// controlador para
        /// </summary>
        /// <param name="userManager">necesario para crear usuario, IdentityUser clse de usuario de autentificacion</param>
        /// <param name="configuration">IConfiguration para otener valores de un proveedor de configuracion </param>
        public UsuariosController(UserManager<Usuario> userManager, IConfiguration configuration
                , SignInManager<Usuario> signInManager, IServicioUsuario servicioUsuario, 
            ApplicationDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.servicioUsuario = servicioUsuario;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy ="esadmin")]
        public async Task<IEnumerable<UsuarioDTO>> Get()
        {
            var usuarios = await context.Users.ToListAsync();
            var usuariosDTO = mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
            return usuariosDTO;
        }


        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login([FromBody] CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            if (usuario is null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuarioDTO.Password!, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuarioDTO);
            }
            else
            {
                return RetornarLoginIncorrecto();
            }



        }

        /// <summary>
        /// Renovar token de usuario
        /// </summary>
        /// <returns></returns>
        [HttpGet("renovar-token")]
        [Authorize]//atributo de seguridad  en este caso se aplica para todo el controlador, y se podria poner el atributo solo para metodo especifico
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await servicioUsuario.ObtenerUsuario();

            if (usuario is null) { return NotFound(); }

            var credencialesUsuarioDTO = new CredencialesUsuarioDTO { Email = usuario.Email! };

            var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO);

            return respuestaAutenticacion;
        }
        /// <summary>
        /// crear usuario admin para que pueda hacer uso del endpoint , controlado por la politica
        /// </summary>
        /// <param name="editarClaimDTO">correo del usuario hacer admin </param>
        /// <returns></returns>
        [HttpPost("hacer-admin")]
        [Authorize("esadmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null) return NotFound();

            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "verdadero"));

            return NoContent();
        }
        /// <summary>
        /// remover  usario del permiso de admin
        /// </summary>
        /// <param name="editarClaimDTO"></param>
        /// <returns></returns>
        [HttpPost("remover-admin")]
        [Authorize("esadmin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null) return NotFound();

            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "verdadero"));

            return NoContent();
        }

        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Login Incorrecto");
            return ValidationProblem();
        }

        [HttpPost("registro")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = new Usuario
            {
                UserName = credencialesUsuarioDTO.Email,
                Email = credencialesUsuarioDTO.Email,

            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password!);
            if (resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO);
                return respuestaAutenticacion;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem();
            }
        }
        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var claims = new List<Claim>
            {
                new Claim("email",credencialesUsuarioDTO.Email),
                new Claim("loqueyoquiera","Cualqueirvalor")
            };
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            var claimDB = await userManager.GetClaimsAsync(usuario!);
            claims.AddRange(claimDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llaveJWT"]!));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenSeguridad = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: credenciales);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion,
            };

        }

        [HttpPut()]
        [Authorize]
        public async Task<ActionResult> Put(ActualizarUsuarioDTO actualizarUsuarioDTO)
        {
            var usuario = await servicioUsuario.ObtenerUsuario();

            if (usuario == null) { return NotFound(); }

            usuario.FechaNacimiento = actualizarUsuarioDTO.FechaNacimiento;

            await userManager.UpdateAsync(usuario);
            return NoContent();
        }
    }
}
