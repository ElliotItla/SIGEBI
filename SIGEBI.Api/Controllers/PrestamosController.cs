using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Prestamos;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Prestamo;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class PrestamosController : ApiControladorBase
{
    private readonly IPrestamoServicio _prestamos;

    public PrestamosController(IPrestamoServicio prestamos)
    {
        _prestamos = prestamos;
    }

    // ---------- Estudiantes y docentes ----------

    /// <summary>Solicita el préstamo de un recurso. Queda pendiente hasta que el personal lo apruebe.</summary>
    /// <remarks>
    /// El sistema comprueba que tu cuenta esté activa, que no tengas penalizaciones ni préstamos vencidos,
    /// que no hayas llegado al máximo de préstamos activos y que el recurso tenga un ejemplar disponible.
    /// </remarks>
    /// <response code="200">Solicitud registrada, en estado Pendiente.</response>
    /// <response code="400">Alguna condición no se cumple. El mensaje explica cuál.</response>
    /// <response code="403">Solo los estudiantes y los docentes piden préstamos.</response>
    [Authorize(Roles = Roles.Lectores)]
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<PrestamoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromBody] SolicitarPrestamoRequest solicitud)
    {
        var resultado = await _prestamos.SolicitarAsync(new SolicitarPrestamoDto
        {
            UsuarioAccionId = UsuarioActualId,
            RecursoId = solicitud.RecursoId
        });

        return Responder(resultado);
    }

    /// <summary>Lista tus solicitudes y préstamos, del más reciente al más antiguo.</summary>
    /// <param name="estado">Pendiente, Activo, Devuelto o Rechazado. Vacío: todos.</param>
    /// <response code="200">Tus préstamos, con la fecha límite y los días que faltan.</response>
    /// <response code="400">El estado indicado no es válido.</response>
    /// <response code="403">Solo los estudiantes y los docentes tienen préstamos propios.</response>
    [Authorize(Roles = Roles.Lectores)]
    [HttpGet("Mios")]
    [ProducesResponseType(typeof(RespuestaApi<List<PrestamoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMios([FromQuery] string? estado)
    {
        return Responder(await _prestamos.ObtenerPorUsuarioAsync(UsuarioActualId, UsuarioActualId, estado));
    }

    // ---------- Personal y administrador ----------

    /// <summary>Lista las solicitudes que esperan aprobación, de la más antigua a la más reciente.</summary>
    /// <response code="200">Solicitudes pendientes.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet("Pendientes")]
    [ProducesResponseType(typeof(RespuestaApi<List<PrestamoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendientes()
    {
        return Responder(await _prestamos.ObtenerPendientesAsync(UsuarioActualId));
    }

    /// <summary>Aprueba una solicitud: el préstamo queda activo, con su fecha límite, y el ejemplar pasa a Prestado.</summary>
    /// <remarks>Antes de aprobar, el sistema vuelve a comprobar las condiciones del usuario.</remarks>
    /// <param name="id">Número del préstamo (aparece en las solicitudes pendientes).</param>
    /// <response code="200">Préstamo aprobado.</response>
    /// <response code="400">La solicitud no existe, ya fue procesada, o el usuario ya no cumple las condiciones.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost("{id:int}/Aprobar")]
    [ProducesResponseType(typeof(RespuestaApi<PrestamoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostAprobar(int id)
    {
        var resultado = await _prestamos.AprobarAsync(new AprobarPrestamoDto
        {
            UsuarioAccionId = UsuarioActualId,
            PrestamoId = id
        });

        return Responder(resultado);
    }

    /// <summary>Rechaza una solicitud indicando el motivo.</summary>
    /// <param name="id">Número del préstamo (aparece en las solicitudes pendientes).</param>
    /// <response code="200">Solicitud rechazada.</response>
    /// <response code="400">Falta el motivo, la solicitud no existe o ya fue procesada.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost("{id:int}/Rechazar")]
    [ProducesResponseType(typeof(RespuestaApi<PrestamoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostRechazar(int id, [FromBody] RechazarPrestamoRequest solicitud)
    {
        var resultado = await _prestamos.RechazarAsync(new RechazarPrestamoDto
        {
            UsuarioAccionId = UsuarioActualId,
            PrestamoId = id,
            Motivo = solicitud.Motivo
        });

        return Responder(resultado);
    }

    // ---------- Consultas del personal, el administrador y el auditor ----------

    /// <summary>Lista los préstamos de un usuario.</summary>
    /// <param name="usuarioId">Número del usuario.</param>
    /// <param name="estado">Pendiente, Activo, Devuelto o Rechazado. Vacío: todos.</param>
    /// <response code="200">Préstamos del usuario.</response>
    /// <response code="400">El estado indicado no es válido.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("Usuario/{usuarioId:int}")]
    [ProducesResponseType(typeof(RespuestaApi<List<PrestamoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPorUsuario(int usuarioId, [FromQuery] string? estado)
    {
        return Responder(await _prestamos.ObtenerPorUsuarioAsync(UsuarioActualId, usuarioId, estado));
    }

    /// <summary>Lista quién ha pedido un recurso y en qué estado está cada préstamo.</summary>
    /// <param name="recursoId">Número del recurso.</param>
    /// <response code="200">Préstamos del recurso.</response>
    /// <response code="400">El recurso no existe.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("Recurso/{recursoId:int}")]
    [ProducesResponseType(typeof(RespuestaApi<List<PrestamoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPorRecurso(int recursoId)
    {
        return Responder(await _prestamos.ObtenerPorRecursoAsync(UsuarioActualId, recursoId));
    }
}