using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Recursos;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Recurso;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class EjemplaresController : ApiControladorBase
{
    private readonly IRecursoServicio _recursos;

    public EjemplaresController(IRecursoServicio recursos)
    {
        _recursos = recursos;
    }

    /// <summary>Cambia el estado de un ejemplar (Disponible, Reservado o FueraDeServicio) y lo deja en su historial.</summary>
    /// <remarks>Un ejemplar prestado no se puede cambiar: su estado cambia al registrar la devolución.</remarks>
    /// <param name="id">Número del ejemplar (aparece en los datos de cada recurso).</param>
    /// <response code="200">Estado cambiado.</response>
    /// <response code="400">Estado no permitido, falta el motivo, el ejemplar no existe, está prestado o ya tenía ese estado.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPut("{id:int}/Estado")]
    [ProducesResponseType(typeof(RespuestaApi<EjemplarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PutEstado(int id, [FromBody] CambiarEstadoEjemplarRequest solicitud)
    {
        var resultado = await _recursos.CambiarEstadoEjemplarAsync(new CambiarEstadoEjemplarDto
        {
            UsuarioAccionId = UsuarioActualId,
            EjemplarId = id,
            EstadoNuevo = solicitud.EstadoNuevo,
            Motivo = solicitud.Motivo
        });

        return Responder(resultado);
    }
}