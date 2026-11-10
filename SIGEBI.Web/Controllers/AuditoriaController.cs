using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Auditoria;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Auditoria;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

// El registro de auditoría es solo de lectura: lo consultan el administrador y el auditor
[Authorize(Roles = Roles.AdminAuditor)]
public class AuditoriaController : ControladorBase
{
    private readonly IAuditoriaServicio _auditoria;

    public AuditoriaController(IAuditoriaServicio auditoria)
    {
        _auditoria = auditoria;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? usuarioId, string? modulo, string? tipoAccion, DateTime? desde, DateTime? hasta)
    {
        var resultado = await _auditoria.ConsultarAsync(Filtro(usuarioId, modulo, tipoAccion, desde, hasta));

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var registros = ListaDe<AuditoriaDto>(resultado);

        return View(new AuditoriaIndexViewModel
        {
            UsuarioId = usuarioId,
            Modulo = modulo,
            TipoAccion = tipoAccion,
            Desde = desde,
            Hasta = hasta,
            Total = registros.Count,
            Registros = registros.Take(AuditoriaIndexViewModel.MaximoEnPantalla).ToList()
        });
    }

    // Descarga todos los registros que cumplen los filtros. Exportar también queda en la auditoría.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Exportar(int? usuarioId, string? modulo, string? tipoAccion, DateTime? desde, DateTime? hasta)
    {
        var resultado = await _auditoria.ExportarCsvAsync(Filtro(usuarioId, modulo, tipoAccion, desde, hasta));

        if (!resultado.Exito || resultado.Datos is not string csv)
        {
            MensajeError(resultado.Mensaje);

            return RedirectToAction(nameof(Index), new { usuarioId, modulo, tipoAccion, desde, hasta });
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