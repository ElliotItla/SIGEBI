using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Auditoria;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Auditoria;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

// El registro de auditoría es solo de lectura: lo consultan el administrador y el auditor
[Authorize(Roles = Roles.AdminAuditor)]
public class AuditoriaController : ApiControladorBase
{
    private readonly IAuditoriaServicio _auditoria;

    public AuditoriaController(IAuditoriaServicio auditoria)
    {
        _auditoria = auditoria;
    }

    /// <summary>Consulta el registro de auditoría: quién hizo qué y cuándo. Solo se puede leer; nadie lo modifica ni lo borra.</summary>
    /// <param name="usuarioId">Número del usuario que realizó la acción.</param>
    /// <param name="modulo">Usuarios, Catalogo, Prestamos, Devoluciones, Penalizaciones, Notificaciones o Reportes.</param>
    /// <param name="tipoAccion">Nombre de la acción, por ejemplo AprobarPrestamo.</param>
    /// <param name="desde">Fecha inicial (yyyy-MM-dd).</param>
    /// <param name="hasta">Fecha final (yyyy-MM-dd).</param>
    /// <response code="200">Registros, del más reciente al más antiguo. Las acciones del sistema salen sin usuario.</response>
    /// <response code="400">El módulo no es válido, o la fecha inicial es posterior a la final.</response>
    /// <response code="403">Solo el administrador y el auditor.</response>
    [HttpGet]
    [ProducesResponseType(typeof(RespuestaApi<List<AuditoriaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] int? usuarioId, [FromQuery] string? modulo, [FromQuery] string? tipoAccion, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        return Responder(await _auditoria.ConsultarAsync(Filtro(usuarioId, modulo, tipoAccion, desde, hasta)));
    }

    /// <summary>Descarga el registro de auditoría como archivo CSV. Exportar también queda registrado.</summary>
    /// <remarks>El cuerpo es opcional: sin filtros se exporta todo el registro.</remarks>
    /// <response code="200">Archivo CSV (se puede abrir en Excel).</response>
    /// <response code="400">El módulo no es válido, o la fecha inicial es posterior a la final.</response>
    /// <response code="403">Solo el administrador y el auditor.</response>
    [HttpPost("Exportar")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostExportar([FromBody] ExportarAuditoriaRequest? solicitud = null)
    {
        var resultado = await _auditoria.ExportarCsvAsync(Filtro(solicitud?.UsuarioId, solicitud?.Modulo, solicitud?.TipoAccion, solicitud?.Desde, solicitud?.Hasta));

        if (!resultado.Exito || resultado.Datos is not string csv)
        {
            return BadRequest(resultado);
        }

        // Con la marca UTF-8 al inicio para que Excel muestre bien las tildes
        var contenido = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

        return File(contenido, "text/csv; charset=utf-8", $"auditoria_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }

    private FiltroAuditoriaDto Filtro(int? usuarioId, string? modulo, string? tipoAccion, DateTime? desde, DateTime? hasta)
    {
        return new FiltroAuditoriaDto
        {
            UsuarioAccionId = UsuarioActualId,
            UsuarioId = usuarioId,
            Modulo = string.IsNullOrWhiteSpace(modulo) ? null : modulo,
            TipoAccion = string.IsNullOrWhiteSpace(tipoAccion) ? null : tipoAccion,
            Desde = desde,
            Hasta = hasta
        };
    }
}