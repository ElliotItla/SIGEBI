using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Notificacion;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class NotificacionesController : ApiControladorBase
{
    private readonly INotificacionServicio _notificaciones;

    public NotificacionesController(INotificacionServicio notificaciones)
    {
        _notificaciones = notificaciones;
    }

    // ---------- Mis notificaciones (cualquier usuario con sesión) ----------

    /// <summary>Lista tus notificaciones, de la más reciente a la más antigua.</summary>
    /// <param name="soloNoLeidas">true muestra solo las que no has leído.</param>
    /// <response code="200">Tus notificaciones. Si no tienes ninguna, la lista viene vacía.</response>
    [HttpGet]
    [ProducesResponseType(typeof(RespuestaApi<List<NotificacionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] bool soloNoLeidas = false)
    {
        return Responder(await _notificaciones.ObtenerMisNotificacionesAsync(UsuarioActualId, soloNoLeidas));
    }

    /// <summary>Cuenta cuántas notificaciones tienes sin leer.</summary>
    /// <response code="200">La cantidad, en el campo datos.</response>
    [HttpGet("NoLeidas")]
    [ProducesResponseType(typeof(RespuestaApi<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNoLeidas()
    {
        return Responder(await _notificaciones.ContarNoLeidasAsync(UsuarioActualId));
    }

    /// <summary>Marca todas tus notificaciones como leídas.</summary>
    /// <response code="200">Notificaciones marcadas.</response>
    [HttpPost("MarcarLeidas")]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostMarcarLeidas()
    {
        return Responder(await _notificaciones.MarcarComoLeidasAsync(UsuarioActualId));
    }

    // ---------- Registro de avisos enviados (administrador y auditor) ----------

    /// <summary>Consulta el registro de todas las notificaciones que el sistema envió, con filtros opcionales.</summary>
    /// <param name="usuarioId">Número del usuario destinatario.</param>
    /// <param name="tipoEvento">ProximoVencimiento, PrestamoSolicitado, PrestamoConfirmado, PrestamoDevuelto, PenalizacionGenerada o PenalizacionResuelta.</param>
    /// <param name="desde">Fecha inicial (yyyy-MM-dd).</param>
    /// <param name="hasta">Fecha final (yyyy-MM-dd).</param>
    /// <response code="200">Notificaciones enviadas, con el nombre del destinatario.</response>
    /// <response code="400">El tipo de evento no es válido, o la fecha inicial es posterior a la final.</response>
    /// <response code="403">Solo el administrador y el auditor.</response>
    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet("Registro")]
    [ProducesResponseType(typeof(RespuestaApi<List<NotificacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRegistro([FromQuery] int? usuarioId, [FromQuery] string? tipoEvento, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var resultado = await _notificaciones.ObtenerRegistroAsync(new FiltroNotificacionDto
        {
            UsuarioAccionId = UsuarioActualId,
            UsuarioId = usuarioId,
            TipoEvento = string.IsNullOrWhiteSpace(tipoEvento) ? null : tipoEvento,
            Desde = desde,
            Hasta = hasta
        });

        return Responder(resultado);
    }

    /// <summary>Envía ahora los avisos de préstamos próximos a vencer, sin esperar a la tarea automática.</summary>
    /// <remarks>
    /// La aplicación web hace esto sola cada hora. Aquí se puede pedir a mano, por ejemplo para una demostración.
    /// No repite avisos: cada préstamo se avisa una sola vez.
    /// </remarks>
    /// <response code="200">Cuántos avisos se enviaron, en el mensaje y en el campo datos.</response>
    /// <response code="403">Solo el administrador.</response>
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost("AvisarVencimientos")]
    [ProducesResponseType(typeof(RespuestaApi<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostAvisarVencimientos()
    {
        return Responder(await _notificaciones.NotificarProximosVencimientosAsync());
    }
}