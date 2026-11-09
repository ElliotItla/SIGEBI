using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.DAL.Base;

namespace SIGEBI.Web.Base;

public abstract class ControladorBase : Controller
{
    // Quién tiene la sesión abierta. Se pasa a la capa de aplicación para saber quién realiza cada acción.
    protected int UsuarioActualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected void MensajeExito(string mensaje)
    {
        TempData["Exito"] = mensaje;
    }

    protected void MensajeError(string mensaje)
    {
        TempData["Error"] = mensaje;
    }

    // Deja el resultado de una operación como mensaje de éxito o de error para la página siguiente
    protected void Avisar(ResultadoOperacion resultado)
    {
        if (resultado.Exito)
        {
            MensajeExito(resultado.Mensaje);
        }
        else
        {
            MensajeError(resultado.Mensaje);
        }
    }

    // Saca la lista que trae un resultado. Si no trae nada (por ejemplo, un rechazo), devuelve una lista vacía.
    protected static List<T> ListaDe<T>(ResultadoOperacion resultado)
    {
        return resultado.Datos as List<T> ?? new List<T>();
    }
}