using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Reporte;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class ReportesController : ApiControladorBase
{
    private readonly IReporteServicio _reportes;

    public ReportesController(IReporteServicio reportes)
    {
        _reportes = reportes;
    }

    // ---------- Reportes por período (administrador y auditor) ----------

    /// <summary>Reporte de préstamos: solicitudes del período, devoluciones a tiempo y préstamos vencidos.</summary>
    /// <remarks>Sin fechas, muestra los últimos 30 días. La tasa de devolución puntual es null si aún no hubo devoluciones.</remarks>
    /// <param name="desde">Fecha inicial (yyyy-MM-dd).</param>
    /// <param name="hasta">Fecha final (yyyy-MM-dd).</param>
    /// <response code="200">El reporte, con el período que se usó.</response>
    /// <response code="400">La fecha inicial es posterior a la final.</response>
    /// <response code="403">Solo el administrador y el auditor.</response>
    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet("Prestamos")]
    [ProducesResponseType(typeof(RespuestaApi<ReportePrestamosDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPrestamos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Responder(await _reportes.ObtenerPrestamosAsync(Filtro(desde, hasta)));
    }

    /// <summary>Reporte de penalizaciones: generadas, resueltas y activas, por tipo de usuario.</summary>
    /// <remarks>Sin fechas, muestra los últimos 30 días.</remarks>
    /// <param name="desde">Fecha inicial (yyyy-MM-dd).</param>
    /// <param name="hasta">Fecha final (yyyy-MM-dd).</param>
    /// <response code="200">El reporte, con el período que se usó.</response>
    /// <response code="400">La fecha inicial es posterior a la final.</response>
    /// <response code="403">Solo el administrador y el auditor.</response>
    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet("Penalizaciones")]
    [ProducesResponseType(typeof(RespuestaApi<ReportePenalizacionesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPenalizaciones([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Responder(await _reportes.ObtenerPenalizacionesAsync(Filtro(desde, hasta)));
    }

    /// <summary>Reporte de uso del catálogo: recursos más solicitados, demanda por categoría y disponibilidad actual.</summary>
    /// <remarks>Sin fechas, muestra los últimos 30 días. La disponibilidad actual es el porcentaje de ejemplares disponibles hoy.</remarks>
    /// <param name="desde">Fecha inicial (yyyy-MM-dd).</param>
    /// <param name="hasta">Fecha final (yyyy-MM-dd).</param>
    /// <response code="200">El reporte, con el período que se usó.</response>
    /// <response code="400">La fecha inicial es posterior a la final.</response>
    /// <response code="403">Solo el administrador y el auditor.</response>
    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet("UsoCatalogo")]
    [ProducesResponseType(typeof(RespuestaApi<ReporteUsoCatalogoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsoCatalogo([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Responder(await _reportes.ObtenerUsoCatalogoAsync(Filtro(desde, hasta)));
    }

    // ---------- Inventario (también el personal) ----------

    /// <summary>Reporte de inventario: cuántos ejemplares hay y en qué estado, por categoría y por recurso.</summary>
    /// <remarks>Es el estado de hoy; no lleva período. No incluye los recursos dados de baja.</remarks>
    /// <response code="200">El inventario actual.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("Inventario")]
    [ProducesResponseType(typeof(RespuestaApi<ReporteInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInventario()
    {
        return Responder(await _reportes.ObtenerInventarioAsync(UsuarioActualId));
    }

    private FiltroReporteDto Filtro(DateTime? desde, DateTime? hasta)
    {
        return new FiltroReporteDto { UsuarioAccionId = UsuarioActualId, Desde = desde, Hasta = hasta };
    }
}