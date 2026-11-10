using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Notificacion;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Notificaciones;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

public class NotificacionesController : ControladorBase
{
    private readonly INotificacionServicio _notificaciones;

    public NotificacionesController(INotificacionServicio notificaciones)
    {
        _notificaciones = notificaciones;
    }

    // ---------- Mis notificaciones (cualquier usuario con sesión) ----------

    [HttpGet]
    public async Task<IActionResult> Index(bool soloNoLeidas)
    {
        var resultado = await _notificaciones.ObtenerMisNotificacionesAsync(UsuarioActualId, soloNoLeidas);
        var conteo = await _notificaciones.ContarNoLeidasAsync(UsuarioActualId);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(new NotificacionesIndexViewModel
        {
            SoloNoLeidas = soloNoLeidas,
            NoLeidas = conteo.Datos is int noLeidas ? noLeidas : 0,
            Notificaciones = ListaDe<NotificacionDto>(resultado)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLeidas()
    {
        var resultado = await _notificaciones.MarcarComoLeidasAsync(UsuarioActualId);

        Avisar(resultado);

        return RedirectToAction(nameof(Index));
    }

    // ---------- Registro de avisos enviados (administrador y auditor) ----------

    [Authorize(Roles = Roles.AdminAuditor)]
    [HttpGet]
    public async Task<IActionResult> Registro(int? usuarioId, string? tipoEvento, DateTime? desde, DateTime? hasta)
    {
        var resultado = await _notificaciones.ObtenerRegistroAsync(new FiltroNotificacionDto
        {
            UsuarioAccionId = UsuarioActualId,
            UsuarioId = usuarioId,
            TipoEvento = string.IsNullOrWhiteSpace(tipoEvento) ? null : tipoEvento,
            Desde = desde,
            Hasta = hasta
        });

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(new RegistroNotificacionesViewModel
        {
            UsuarioId = usuarioId,
            TipoEvento = tipoEvento,
            Desde = desde,
            Hasta = hasta,
            Notificaciones = ListaDe<NotificacionDto>(resultado)
        });
    }
}