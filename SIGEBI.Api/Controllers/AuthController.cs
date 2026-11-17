using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Usuarios;
using SIGEBI.Api.Respuestas;
using SIGEBI.BL.Dtos.Usuario;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;

namespace SIGEBI.Api.Controllers;

public class AuthController : ApiControladorBase
{
    private readonly IUsuarioServicio _usuarios;

    public AuthController(IUsuarioServicio usuarios)
    {
        _usuarios = usuarios;
    }

    /// <summary>Inicia sesión y devuelve el token que se envía en las demás peticiones.</summary>
    /// <remarks>
    /// El token se manda en el encabezado <c>Authorization: Bearer &lt;token&gt;</c>. En Swagger, pégalo en el botón Authorize.
    /// Después de 5 intentos fallidos la cuenta se bloquea unos minutos.
    /// </remarks>
    /// <response code="200">Sesión iniciada: devuelve el accessToken.</response>
    /// <response code="400">Faltan datos.</response>
    /// <response code="401">Correo o contraseña incorrectos, cuenta desactivada o bloqueada.</response>
    [AllowAnonymous]
    [HttpPost("Login")]
    [ProducesResponseType(typeof(SesionRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultadoOperacion), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest solicitud)
    {
        var resultado = await _usuarios.IniciarSesionAsync(new IniciarSesionDto
        {
            Correo = solicitud.Correo,
            Contrasena = solicitud.Contrasena
        });

        if (!resultado.Exito || resultado.Datos is not UsuarioDto usuario)
        {
            return Unauthorized(resultado);
        }

        // El token guarda quién es el usuario y su rol (el tipo de usuario)
        var datos = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Tipo)
        };

        var identidad = new ClaimsIdentity(datos, BearerTokenDefaults.AuthenticationScheme);

        return SignIn(new ClaimsPrincipal(identidad), BearerTokenDefaults.AuthenticationScheme);
    }

    /// <summary>Muestra quién es el usuario que envió el token.</summary>
    /// <response code="200">Id, nombre, correo y rol del usuario.</response>
    [HttpGet("Yo")]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status200OK)]
    public IActionResult Yo()
    {
        return Ok(new ResultadoOperacion
        {
            Exito = true,
            Mensaje = "Sesión válida.",
            Datos = new
            {
                id = UsuarioActualId,
                nombre = User.FindFirstValue(ClaimTypes.Name),
                correo = User.FindFirstValue(ClaimTypes.Email),
                rol = User.FindFirstValue(ClaimTypes.Role)
            }
        });
    }
}