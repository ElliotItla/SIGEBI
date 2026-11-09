using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Recurso;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Catalogo;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

// Todos los usuarios con sesión consultan el catálogo. Lo demás depende del rol.
public class CatalogoController : ControladorBase
{
    private readonly IRecursoServicio _recursos;

    public CatalogoController(IRecursoServicio recursos)
    {
        _recursos = recursos;
    }

    // ---------- Consultar ----------

    [HttpGet]
    public async Task<IActionResult> Index(string? titulo, string? autor, string? categoria, bool soloDisponibles)
    {
        var resultado = await _recursos.BuscarAsync(new FiltroRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Titulo = titulo,
            Autor = autor,
            Categoria = categoria,
            SoloDisponibles = soloDisponibles
        });

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(new CatalogoIndexViewModel
        {
            Titulo = titulo,
            Autor = autor,
            Categoria = categoria,
            SoloDisponibles = soloDisponibles,
            Recursos = ListaDe<RecursoDto>(resultado),
            PuedeAdministrar = User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Personal)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var resultado = await _recursos.ObtenerPorIdAsync(UsuarioActualId, id);

        if (resultado.Datos is not RecursoDto recurso)
        {
            MensajeError(resultado.Mensaje);

            return RedirectToAction(nameof(Index));
        }

        return View(new DetalleRecursoViewModel
        {
            Recurso = recurso,
            PuedeAdministrar = User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Personal),
            PuedeDarDeBaja = User.IsInRole(Roles.Administrador),
            PuedeVerHistorial = User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Personal) || User.IsInRole(Roles.Auditor),
            PuedeSolicitar = User.IsInRole(Roles.Estudiante) || User.IsInRole(Roles.Docente)
        });
    }

    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet]
    public async Task<IActionResult> Historial(int id)
    {
        var resultado = await _recursos.ObtenerHistorialAsync(UsuarioActualId, id);

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);

            return RedirectToAction(nameof(Index));
        }

        // El título es solo para el encabezado: si el recurso ya no existe, se muestra el historial igual
        var recurso = (await _recursos.ObtenerPorIdAsync(UsuarioActualId, id)).Datos as RecursoDto;

        return View(new HistorialRecursoViewModel
        {
            RecursoId = id,
            Recurso = recurso,
            Cambios = ListaDe<HistorialEjemplarDto>(resultado)
        });
    }

    // ---------- Registrar ----------

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new RecursoFormViewModel());
    }

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(RecursoFormViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Codigo))
        {
            ModelState.AddModelError(nameof(modelo.Codigo), "Escribe el código del recurso.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _recursos.GuardarAsync(new GuardarRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Codigo = modelo.Codigo!,
            Titulo = modelo.Titulo,
            Autor = modelo.Autor,
            Categoria = modelo.Categoria,
            CantidadEjemplares = modelo.CantidadEjemplares
        });

        if (!resultado.Exito || resultado.Datos is not RecursoDto nuevo)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);

            return View(modelo);
        }

        MensajeExito(resultado.Mensaje);

        return RedirectToAction(nameof(Detalle), new { id = nuevo.Id });
    }

    // ---------- Editar ----------

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var resultado = await _recursos.ObtenerPorIdAsync(UsuarioActualId, id);

        if (resultado.Datos is not RecursoDto recurso)
        {
            MensajeError(resultado.Mensaje);

            return RedirectToAction(nameof(Index));
        }

        return View(new RecursoFormViewModel
        {
            Id = recurso.Id,
            Codigo = recurso.Codigo,
            Titulo = recurso.Titulo,
            Autor = recurso.Autor,
            Categoria = recurso.Categoria
        });
    }

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(RecursoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _recursos.ActualizarAsync(new ActualizarRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = modelo.Id,
            Titulo = modelo.Titulo,
            Autor = modelo.Autor,
            Categoria = modelo.Categoria
        });

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);

            return View(modelo);
        }

        MensajeExito(resultado.Mensaje);

        return RedirectToAction(nameof(Detalle), new { id = modelo.Id });
    }

    // ---------- Ejemplares y baja ----------

    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoEjemplar(CambiarEstadoEjemplarViewModel modelo)
    {
        var resultado = await _recursos.CambiarEstadoEjemplarAsync(new CambiarEstadoEjemplarDto
        {
            UsuarioAccionId = UsuarioActualId,
            EjemplarId = modelo.EjemplarId,
            EstadoNuevo = modelo.EstadoNuevo,
            Motivo = modelo.Motivo
        });

        Avisar(resultado);

        return RedirectToAction(nameof(Detalle), new { id = modelo.RecursoId });
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DarDeBaja(int id)
    {
        var resultado = await _recursos.DarDeBajaAsync(new BajaRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = id
        });

        Avisar(resultado);

        // Si se dio de baja, el recurso ya no existe en el catálogo; si no, se vuelve a su página
        return resultado.Exito
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Detalle), new { id });
    }
}