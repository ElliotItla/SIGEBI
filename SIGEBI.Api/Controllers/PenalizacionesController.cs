using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Penalizaciones;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Penalizacion;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class PenalizacionesController : ApiControladorBase
{
    private readonly IPenalizacionServicio _penalizaciones;

    public PenalizacionesController(IPenalizacionServicio penalizaciones)
    {
        _penalizaciones = penalizaciones;
    }

    // ---------- Estudiantes y docentes ----------

    /// <summary>Lista tus penalizaciones, de la más reciente a la más antigua.</summary>
    /// <remarks>Mientras tengas una penalización activa no puedes pedir préstamos.</remarks>
    /// <param name="soloActivas">true muestra solo las activas. Por defecto muestra también las resueltas.</param>
    /// <response code="200">Tus penalizaciones. Si no tienes ninguna, la lista viene vacía.</response>
    /// <response code="403">Solo los estudiantes y los docentes tienen penalizaciones propias.</response>
    [Authorize(Roles = Roles.Lectores)]
    [HttpGet("Mias")]
    [ProducesResponseType(typeof(RespuestaApi<List<PenalizacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMias([FromQuery] bool soloActivas = false)
    {
        return Responder(await _penalizaciones.ObtenerPorUsuarioAsync(UsuarioActualId, UsuarioActualId, soloActivas));
    }

    // ---------- Personal y administrador ----------

    /// <summary>Lista todas las penalizaciones activas, de la más antigua a la más reciente.</summary>
    /// <response code="200">Penalizaciones activas de todos los usuarios.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpGet("Activas")]
    [ProducesResponseType(typeof(RespuestaApi<List<PenalizacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivas()
    {
        return Responder(await _penalizaciones.ObtenerActivasAsync(UsuarioActualId));
    }

    /// <summary>Resuelve una penalización activa. El usuario puede volver a pedir préstamos y recibe un aviso.</summary>
    /// <param name="id">Número de la penalización (aparece en la lista de activas).</param>
    /// <response code="200">Penalización resuelta, con su fecha y su motivo.</response>
    /// <response code="400">Falta el motivo, la penalización no existe o ya estaba resuelta.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost("{id:int}/Resolver")]
    [ProducesResponseType(typeof(RespuestaApi<PenalizacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PostResolver(int id, [FromBody] ResolverPenalizacionRequest solicitud)
    {
        var resultado = await _penalizaciones.ResolverAsync(new ResolverPenalizacionDto
        {
            UsuarioAccionId = UsuarioActualId,
            PenalizacionId = id,
            Motivo = solicitud.Motivo
        });

        return Responder(resultado);
    }

    // ---------- Consulta del personal, el administrador y el auditor ----------

    /// <summary>Lista las penalizaciones de un usuario, activas y resueltas.</summary>
    /// <param name="usuarioId">Número del usuario.</param>
    /// <param name="soloActivas">true muestra solo las activas.</param>
    /// <response code="200">Penalizaciones del usuario. Si no tiene ninguna, la lista viene vacía.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("Usuario/{usuarioId:int}")]
    [ProducesResponseType(typeof(RespuestaApi<List<PenalizacionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPorUsuario(int usuarioId, [FromQuery] bool soloActivas = false)
    {
        return Responder(await _penalizaciones.ObtenerPorUsuarioAsync(UsuarioActualId, usuarioId, soloActivas));
    }
}