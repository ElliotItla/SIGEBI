using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Prestamo;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Prestamos;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

public class PrestamosController : ControladorBase
{
    private readonly IPrestamoServicio _prestamos;

    public PrestamosController(IPrestamoServicio prestamos)
    {
        _prestamos = prestamos;
    }

    // ---------- Estudiantes y docentes ----------

    [Authorize(Roles = Roles.Lectores)]
    [HttpGet]
    public async Task<IActionResult> Mios(string? estado)
    {
        var resultado = await _prestamos.ObtenerPorUsuarioAsync(UsuarioActualId, UsuarioActualId, EstadoONulo(estado));

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View("Lista", new PrestamosListaViewModel
        {
            Titulo = "Mis préstamos",
            Ayuda = "Tus solicitudes y préstamos, con las fechas de entrega.",
            Estado = estado,
            MostrarFiltroEstado = true,
            Prestamos = ListaDe<PrestamoDto>(resultado)
        });
    }

    [Authorize(Roles = Roles.Lectores)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Solicitar(int recursoId)
    {
        var resultado = await _prestamos.SolicitarAsync(new SolicitarPrestamoDto
        {
            UsuarioAccionId = UsuarioActualId,
            RecursoId = recursoId
        });

        Avisar(resultado);

        // Si se registró, se muestra en "Mis préstamos"; si no, se vuelve al recurso con el motivo
        return resultado.Exito
            ? RedirectToAction(nameof(Mios))
            : RedirectToAction("Detalle", "Catalogo", new { id = recursoId });
    }

    // ---------- Personal y administrador ----------

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet]
    public async Task<IActionResult> Pendientes()
    {
        var resultado = await _prestamos.ObtenerPendientesAsync(UsuarioActualId);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(ListaDe<PrestamoDto>(resultado));
    }

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int prestamoId)
    {
        var resultado = await _prestamos.AprobarAsync(new AprobarPrestamoDto
        {
            UsuarioAccionId = UsuarioActualId,
            PrestamoId = prestamoId
        });

        Avisar(resultado);

        return RedirectToAction(nameof(Pendientes));
    }

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int prestamoId, string? motivo)
    {
        var resultado = await _prestamos.RechazarAsync(new RechazarPrestamoDto
        {
            UsuarioAccionId = UsuarioActualId,
            PrestamoId = prestamoId,
            Motivo = motivo ?? string.Empty
        });

        Avisar(resultado);

        return RedirectToAction(nameof(Pendientes));
    }

    // ---------- Consultas del personal, el administrador y el auditor ----------

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> PorUsuario(int id, string? estado)
    {
        var resultado = await _prestamos.ObtenerPorUsuarioAsync(UsuarioActualId, id, EstadoONulo(estado));

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var prestamos = ListaDe<PrestamoDto>(resultado);
        var nombre = prestamos.Select(p => p.NombreUsuario).FirstOrDefault(n => n != null) ?? $"el usuario {id}";

        return View("Lista", new PrestamosListaViewModel
        {
            Titulo = "Préstamos de " + nombre,
            Ayuda = "Historial de solicitudes y préstamos de este usuario.",
            Estado = estado,
            MostrarFiltroEstado = true,
            Prestamos = prestamos
        });
    }

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> PorRecurso(int id)
    {
        var resultado = await _prestamos.ObtenerPorRecursoAsync(UsuarioActualId, id);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var prestamos = ListaDe<PrestamoDto>(resultado);
        var titulo = prestamos.Select(p => p.TituloRecurso).FirstOrDefault(t => t != null) ?? $"el recurso {id}";

        return View("Lista", new PrestamosListaViewModel
        {
            Titulo = "Préstamos de " + titulo,
            Ayuda = "Quién ha pedido este recurso y en qué estado está cada préstamo.",
            MostrarUsuario = true,
            MostrarRecurso = false,
            Prestamos = prestamos
        });
    }

    // Un valor vacío del filtro significa "todos"
    private static string? EstadoONulo(string? estado)
    {
        return string.IsNullOrWhiteSpace(estado) ? null : estado;
    }
}