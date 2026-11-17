using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;

namespace SIGEBI.Api.Controllers;

public class EstadoController : ApiControladorBase
{
    /// <summary>Comprueba que la API está en funcionamiento. No pide token.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new { nombre = "SIGEBI API", version = "1.0", fecha = DateTime.Now });
    }
}