using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Usuario;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Usuarios;
using SIGEBI.Web.Seguridad;

namespace SIGEBI.Web.Controllers;

// Consultar la lista es del administrador y del personal; lo demás, solo del administrador
[Authorize(Roles = Roles.AdminPersonal)]
public class UsuariosController : ControladorBase
{
    private readonly IUsuarioServicio _usuarios;

    public UsuariosController(IUsuarioServicio usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? tipo, string? estado)
    {
        bool? activo = estado == "activos" ? true : estado == "inactivos" ? false : null;

        var resultado = await _usuarios.ObtenerPorFiltroAsync(new FiltroUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Tipo = string.IsNullOrEmpty(tipo) ? null : tipo,
            Activo = activo
        });

        if (!resultado.Exito)
        {
            MensajeError(resultado.Mensaje);
        }

        return View(new UsuariosIndexViewModel
        {
            Tipo = tipo,
            Estado = estado,
            Usuarios = ListaDe<UsuarioDto>(resultado),
            PuedeAdministrar = User.IsInRole(Roles.Administrador)
        });
    }

    // ---------- Registrar ----------

    [Authorize(Roles = Roles.Administrador)]
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new UsuarioFormViewModel());
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioFormViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Identificador))
        {
            ModelState.AddModelError(nameof(modelo.Identificador), "Escribe el identificador.");
        }

        if (string.IsNullOrEmpty(modelo.Contrasena))
        {
            ModelState.AddModelError(nameof(modelo.Contrasena), "Escribe una contraseña.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _usuarios.GuardarAsync(new GuardarUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Identificador = modelo.Identificador!,
            Nombre = modelo.Nombre,
            Apellido = modelo.Apellido,
            Correo = modelo.Correo,
            Tipo = modelo.Tipo,
            Contrasena = modelo.Contrasena!
        });

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);

            return View(modelo);
        }

        MensajeExito(resultado.Mensaje);

        return RedirectToAction(nameof(Index));
    }

    // ---------- Editar ----------

    [Authorize(Roles = Roles.Administrador)]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var resultado = await _usuarios.ObtenerPorIdAsync(id);

        if (resultado.Datos is not UsuarioDto usuario)
        {
            MensajeError(resultado.Mensaje);

            return RedirectToAction(nameof(Index));
        }

        return View(new UsuarioFormViewModel
        {
            Id = usuario.Id,
            Identificador = usuario.Identificador,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Tipo = usuario.Tipo
        });
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(UsuarioFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _usuarios.ActualizarAsync(new ActualizarUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = modelo.Id,
            Nombre = modelo.Nombre,
            Apellido = modelo.Apellido,
            Correo = modelo.Correo,
            Tipo = modelo.Tipo,
            NuevaContrasena = modelo.Contrasena
        });

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);

            return View(modelo);
        }

        MensajeExito(resultado.Mensaje);

        return RedirectToAction(nameof(Index));
    }

    // ---------- Activar o desactivar ----------

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, bool activo)
    {
        var resultado = await _usuarios.CambiarEstadoAsync(new CambiarEstadoUsuarioDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = id,
            Activo = activo
        });

        Avisar(resultado);

        return RedirectToAction(nameof(Index));
    }
}