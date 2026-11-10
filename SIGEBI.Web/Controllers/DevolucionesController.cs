using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Devolucion;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Devoluciones;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

public class DevolucionesController : ControladorBase
{
    private const string ClaveResultado = "UltimaDevolucion";

    private readonly IDevolucionServicio _devoluciones;

    public DevolucionesController(IDevolucionServicio devoluciones)
    {
        _devoluciones = devoluciones;
    }

    // ---------- Registrar (personal y administrador) ----------

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet]
    public IActionResult Registrar()
    {
        var modelo = new RegistrarDevolucionViewModel();

        // Resumen de la devolución que se acaba de registrar (viene de la página anterior)
        if (TempData[ClaveResultado] is string json)
        {
            modelo.Resultado = JsonSerializer.Deserialize<DevolucionDto>(json);
        }

        return View(modelo);
    }

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarDevolucionViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _devoluciones.RegistrarAsync(new RegistrarDevolucionDto
        {
            UsuarioAccionId = UsuarioActualId,
            CodigoEjemplar = modelo.CodigoEjemplar,
            Danado = modelo.Danado,
            Observacion = modelo.Observacion
        });

        if (!resultado.Exito || resultado.Datos is not DevolucionDto devolucion)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);

            return View(modelo);
        }

        MensajeExito(resultado.Mensaje);
        TempData[ClaveResultado] = JsonSerializer.Serialize(devolucion);

        return RedirectToAction(nameof(Registrar));
    }

    // ---------- Historial (personal, administrador y auditor) ----------

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> PorUsuario(int id)
    {
        var resultado = await _devoluciones.ObtenerHistorialPorUsuarioAsync(UsuarioActualId, id);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var devoluciones = ListaDe<DevolucionDto>(resultado);
        var nombre = devoluciones.Select(d => d.NombreUsuario).FirstOrDefault(n => n != null) ?? $"el usuario {id}";

        return View("Historial", new HistorialDevolucionesViewModel
        {
            Titulo = "Devoluciones de " + nombre,
            Ayuda = "Recursos que este usuario ya devolvió.",
            Devoluciones = devoluciones
        });
    }

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> PorRecurso(int id)
    {
        var resultado = await _devoluciones.ObtenerHistorialPorRecursoAsync(UsuarioActualId, id);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        var devoluciones = ListaDe<DevolucionDto>(resultado);
        var titulo = devoluciones.Select(d => d.TituloRecurso).FirstOrDefault(t => t != null) ?? $"el recurso {id}";

        return View("Historial", new HistorialDevolucionesViewModel
        {
            Titulo = "Devoluciones de " + titulo,
            Ayuda = "Quién devolvió este recurso y cuándo.",
            MostrarUsuario = true,
            MostrarRecurso = false,
            Devoluciones = devoluciones
        });
    }
}