using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.BL.Dtos.Usuario;
using SIGEBI.BL.Interfaces;
using SIGEBI.Web.Base;
using SIGEBI.Web.Models.Usuarios;

namespace SIGEBI.Web.Controllers;

public class CuentaController : ControladorBase
{
    private readonly IUsuarioServicio _usuarios;

    public CuentaController(IUsuarioServicio usuarios)
    {
        _usuarios = usuarios;
    }

    // ---------- Iniciar y cerrar sesión ----------

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Ingresar(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        // Si todavía no hay ningún usuario, se crea el administrador inicial
        if (!await HayUsuariosAsync())
        {
            return RedirectToAction(nameof(ConfigurarInicial));
        }

        return View(new IngresarViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingresar(IngresarViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _usuarios.IniciarSesionAsync(new IniciarSesionDto
        {
            Correo = modelo.Correo,
            Contrasena = modelo.Contrasena
        });

        if (!resultado.Exito || resultado.Datos is not UsuarioDto usuario)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            modelo.Contrasena = string.Empty;

            return View(modelo);
        }

        // La sesión guarda quién es el usuario y su rol (el tipo de usuario)
        var datos = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Tipo)
        };

        var identidad = new ClaimsIdentity(datos, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));

        // Solo se vuelve a páginas de este mismo sitio
        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
        {
            return LocalRedirect(modelo.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Ingresar));
    }

    // ---------- Primer administrador ----------

    // Solo funciona mientras el sistema no tenga ningún usuario
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfigurarInicial()
    {
        if (await HayUsuariosAsync())
        {
            return RedirectToAction(nameof(Ingresar));
        }

        return View(new AdministradorInicialViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigurarInicial(AdministradorInicialViewModel modelo)
    {
        if (await HayUsuariosAsync())
        {
            return RedirectToAction(nameof(Ingresar));
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _usuarios.CrearAdministradorInicialAsync(
            modelo.Identificador, modelo.Nombre, modelo.Apellido, modelo.Correo, modelo.Contrasena);

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);

            return View(modelo);
        }

        MensajeExito("Se creó el administrador. Ya puedes iniciar sesión.");

        return RedirectToAction(nameof(Ingresar));
    }

    private async Task<bool> HayUsuariosAsync()
    {
        var resultado = await _usuarios.ObtenerTodosAsync();

        return ListaDe<UsuarioDto>(resultado).Count > 0;
    }
}