using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Penalizacion;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Penalizaciones;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

public class PenalizacionesController : ControladorBase
{
    private readonly IPenalizacionServicio _penalizaciones;

    public PenalizacionesController(IPenalizacionServicio penalizaciones)
    {
        _penalizaciones = penalizaciones;
    }

    // ---------- Estudiantes y docentes ----------

    [Authorize(Roles = Roles.Lectores)]
    [HttpGet]
    public async Task<IActionResult> Mias(bool soloActivas)
    {
        var resultado = await _penalizaciones.ObtenerPorUsuarioAsync(UsuarioActualId, UsuarioActualId, soloActivas);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View("Lista", new PenalizacionesListaViewModel
        {
            Titulo = "Mis penalizaciones",
            Ayuda = "Se generan cuando devuelves un recurso después de la fecha límite.",
            SoloActivas = soloActivas,
            AvisarBloqueo = true,
            Penalizaciones = ListaDe<PenalizacionDto>(resultado)
        });
    }

    // ---------- Personal y administrador ----------

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet]
    public async Task<IActionResult> Activas()
    {
        var resultado = await _penalizaciones.ObtenerActivasAsync(UsuarioActualId);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(ListaDe<PenalizacionDto>(resultado));
    }

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolver(int penalizacionId, string? motivo)
    {
        var resultado = await _penalizaciones.ResolverAsync(new ResolverPenalizacionDto
        {
            UsuarioAccionId = UsuarioActualId,
            PenalizacionId = penalizacionId,
            Motivo = motivo ?? string.Empty
        });

        Avisar(resultado);

        return RedirectToAction(nameof(Activas));
    }

    // ---------- Consulta del personal, el administrador y el auditor ----------

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> PorUsuario(int id, bool soloActivas)
    {
        var resultado = await _penalizaciones.ObtenerPorUsuarioAsync(UsuarioActualId, id, soloActivas);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var penalizaciones = ListaDe<PenalizacionDto>(resultado);
        var nombre = penalizaciones.Select(p => p.NombreUsuario).FirstOrDefault(n => n != null) ?? $"el usuario {id}";

        return View("Lista", new PenalizacionesListaViewModel
        {
            Titulo = "Penalizaciones de " + nombre,
            Ayuda = "Activas y resueltas de este usuario.",
            SoloActivas = soloActivas,
            Penalizaciones = penalizaciones
        });
    }
}