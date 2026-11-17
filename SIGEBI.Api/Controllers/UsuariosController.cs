using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Usuarios;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Usuario;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class UsuariosController : ApiControladorBase
{
    private readonly IUsuarioServicio _usuarios;

    public UsuariosController(IUsuarioServicio usuarios)
    {
        _usuarios = usuarios;
    }

    /// <summary>Lista los usuarios, con filtros opcionales.</summary>
    /// <param name="tipo">Estudiante, Docente, PersonalBibliotecario, Administrador o Auditor. Vacío: todos.</param>
    /// <param name="activo">true solo los activos, false solo los inactivos. Vacío: todos.</param>
    /// <response code="200">Lista de usuarios.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet]
    [ProducesResponseType(typeof(RespuestaApi<List<UsuarioDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] string? tipo, [FromQuery] bool? activo)
    {
        var resultado = await _usuarios.ObtenerPorFiltroAsync(new FiltroUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Tipo = string.IsNullOrWhiteSpace(tipo) ? null : tipo,
            Activo = activo
        });

        return Responder(resultado);
    }

    /// <summary>Obtiene un usuario por su número.</summary>
    /// <param name="id">Número del usuario.</param>
    /// <response code="200">El usuario.</response>
    /// <response code="400">El usuario no existe.</response>
    /// <response code="403">Solo el administrador.</response>
    [Authorize(Roles = Roles.Administrador)]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(int id)
    {
        return Responder(await _usuarios.ObtenerPorIdAsync(id));
    }

    /// <summary>Registra un usuario nuevo. Queda activo.</summary>
    /// <response code="200">Usuario registrado.</response>
    /// <response code="400">Datos inválidos, o ya existe un usuario con ese correo o identificador.</response>
    /// <response code="403">Solo el administrador.</response>
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromBody] CrearUsuarioRequest solicitud)
    {
        var resultado = await _usuarios.GuardarAsync(new GuardarUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Identificador = solicitud.Identificador,
            Nombre = solicitud.Nombre,
            Apellido = solicitud.Apellido,
            Correo = solicitud.Correo,
            Tipo = solicitud.Tipo,
            Contrasena = solicitud.Contrasena
        });

        return Responder(resultado);
    }

    /// <summary>Actualiza los datos de un usuario. El identificador no se cambia.</summary>
    /// <param name="id">Número del usuario.</param>
    /// <response code="200">Usuario actualizado.</response>
    /// <response code="400">Datos inválidos, el usuario no existe o el correo ya lo usa otro.</response>
    /// <response code="403">Solo el administrador.</response>
    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Put(int id, [FromBody] ActualizarUsuarioRequest solicitud)
    {
        var resultado = await _usuarios.ActualizarAsync(new ActualizarUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = id,
            Nombre = solicitud.Nombre,
            Apellido = solicitud.Apellido,
            Correo = solicitud.Correo,
            Tipo = solicitud.Tipo,
            NuevaContrasena = solicitud.NuevaContrasena
        });

        return Responder(resultado);
    }

    /// <summary>Activa o desactiva la cuenta de un usuario.</summary>
    /// <param name="id">Número del usuario.</param>
    /// <response code="200">Estado cambiado.</response>
    /// <response code="400">El usuario no existe, o intentas desactivar tu propia cuenta.</response>
    /// <response code="403">Solo el administrador.</response>
    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{id:int}/Estado")]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PutEstado(int id, [FromBody] CambiarEstadoUsuarioRequest solicitud)
    {
        var resultado = await _usuarios.CambiarEstadoAsync(new CambiarEstadoUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = id,
            Activo = solicitud.Activo
        });

        return Responder(resultado);
    }
}