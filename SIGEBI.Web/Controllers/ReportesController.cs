using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Reporte;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Reportes;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

public class ReportesController : ControladorBase
{
    private readonly IReporteServicio _reportes;

    public ReportesController(IReporteServicio reportes)
    {
        _reportes = reportes;
    }

    // Cada rol ve los reportes que le corresponden
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // ---------- Reportes por período (administrador y auditor) ----------

    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet]
    public async Task<IActionResult> Prestamos(DateTime? desde, DateTime? hasta)
    {
        var resultado = await _reportes.ObtenerPrestamosAsync(Filtro(desde, hasta));

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var reporte = resultado.Datos as ReportePrestamosDto;

        return View(new ReporteViewModel<ReportePrestamosDto>
        {
            Desde = reporte?.Desde ?? desde,
            Hasta = reporte?.Hasta ?? hasta,
            Datos = reporte
        });
    }

    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet]
    public async Task<IActionResult> Penalizaciones(DateTime? desde, DateTime? hasta)
    {
        var resultado = await _reportes.ObtenerPenalizacionesAsync(Filtro(desde, hasta));

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var reporte = resultado.Datos as ReportePenalizacionesDto;

        return View(new ReporteViewModel<ReportePenalizacionesDto>
        {
            Desde = reporte?.Desde ?? desde,
            Hasta = reporte?.Hasta ?? hasta,
            Datos = reporte
        });
    }

    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet]
    public async Task<IActionResult> UsoCatalogo(DateTime? desde, DateTime? hasta)
    {
        var resultado = await _reportes.ObtenerUsoCatalogoAsync(Filtro(desde, hasta));

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var reporte = resultado.Datos as ReporteUsoCatalogoDto;

        return View(new ReporteViewModel<ReporteUsoCatalogoDto>
        {
            Desde = reporte?.Desde ?? desde,
            Hasta = reporte?.Hasta ?? hasta,
            Datos = reporte
        });
    }

    // ---------- Inventario (también el personal) ----------

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> Inventario()
    {
        var resultado = await _reportes.ObtenerInventarioAsync(UsuarioActualId);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(new ReporteViewModel<ReporteInventarioDto> { Datos = resultado.Datos as ReporteInventarioDto });
    }

    private FiltroReporteDto Filtro(DateTime? desde, DateTime? hasta)
    {
        return new FiltroReporteDto { UsuarioAccionId = UsuarioActualId, Desde = desde, Hasta = hasta };
    }
}