using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.DAL.Base;

namespace SIGEBI.Api.Base;

// Base de todos los controladores: la ruta es api/<Nombre del controlador>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ResultadoOperacion), StatusCodes.Status400BadRequest)]
public abstract class ApiControladorBase : ControllerBase
{
    // Quién envió el token. Se pasa a la capa de aplicación para saber quién realiza cada acción.
    protected int UsuarioActualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Éxito: 200 con el resultado. Rechazo de la capa de aplicación: 400 con el motivo.
    protected IActionResult Responder(ResultadoOperacion resultado)
    {
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}