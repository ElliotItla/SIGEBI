using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Devoluciones;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Devolucion;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class DevolucionesController : ApiControladorBase
{
    private readonly IDevolucionServicio _devoluciones;

    public DevolucionesController(IDevolucionServicio devoluciones)
    {
        _devoluciones = devoluciones;
    }

    /// <summary>Registra la devolución de un ejemplar prestado.</summary>
    /// <remarks>
    /// Cierra el préstamo y deja el ejemplar Disponible (o Fuera de servicio si llegó dañado).
    /// Si la devolución es tardía, el sistema genera la penalización y avisa al usuario.
    /// </remarks>
    /// <response code="200">Devolución registrada. Indica los días de retraso, si se generó una penalización y el estado del ejemplar.</response>
    /// <response code="400">Falta el código, el ejemplar no existe o no tiene un préstamo activo, o la observación es demasiado larga.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<DevolucionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromBody] RegistrarDevolucionRequest solicitud)
    {
        var resultado = await _devoluciones.RegistrarAsync(new RegistrarDevolucionDto
        {
            UsuarioAccionId = UsuarioActualId,
            CodigoEjemplar = solicitud.CodigoEjemplar,
            Danado = solicitud.Danado,
            Observacion = solicitud.Observacion
        });

        return Responder(resultado);
    }

    /// <summary>Lista las devoluciones que ha hecho un usuario.</summary>
    /// <param name="usuarioId">Número del usuario.</param>
    /// <response code="200">Historial de devoluciones del usuario. Si no ha devuelto nada, la lista viene vacía.</response>
    /// <response code="400">El usuario no existe.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("Usuario/{usuarioId:int}")]
    [ProducesResponseType(typeof(RespuestaApi<List<DevolucionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPorUsuario(int usuarioId)
    {
        return Responder(await _devoluciones.ObtenerHistorialPorUsuarioAsync(UsuarioActualId, usuarioId));
    }

    /// <summary>Lista quién ha devuelto un recurso y cuándo.</summary>
    /// <param name="recursoId">Número del recurso.</param>
    /// <response code="200">Historial de devoluciones del recurso.</response>
    /// <response code="400">El recurso no existe.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("Recurso/{recursoId:int}")]
    [ProducesResponseType(typeof(RespuestaApi<List<DevolucionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPorRecurso(int recursoId)
    {
        return Responder(await _devoluciones.ObtenerHistorialPorRecursoAsync(UsuarioActualId, recursoId));
    }
}