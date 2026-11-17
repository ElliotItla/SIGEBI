using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Api.Base;
using SIGEBI.Api.Requests.Recursos;
using SIGEBI.Api.Respuestas;
using SIGEBI.Api.Seguridad;
using SIGEBI.BL.Dtos.Recurso;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.Api.Controllers;

public class RecursosController : ApiControladorBase
{
    private readonly IRecursoServicio _recursos;

    public RecursosController(IRecursoServicio recursos)
    {
        _recursos = recursos;
    }

    /// <summary>Busca recursos en el catálogo, con filtros opcionales. Lo puede usar cualquier usuario con sesión.</summary>
    /// <param name="titulo">Parte del título.</param>
    /// <param name="autor">Parte del nombre del autor.</param>
    /// <param name="categoria">Categoría exacta.</param>
    /// <param name="soloDisponibles">true muestra solo los recursos con algún ejemplar disponible.</param>
    /// <response code="200">Lista de recursos con la cantidad de ejemplares disponibles. Si no hay coincidencias, la lista viene vacía.</response>
    [HttpGet]
    [ProducesResponseType(typeof(RespuestaApi<List<RecursoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? titulo, [FromQuery] string? autor, [FromQuery] string? categoria, [FromQuery] bool soloDisponibles = false)
    {
        var resultado = await _recursos.BuscarAsync(new FiltroRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Titulo = titulo,
            Autor = autor,
            Categoria = categoria,
            SoloDisponibles = soloDisponibles
        });

        return Responder(resultado);
    }

    /// <summary>Obtiene un recurso con todos sus ejemplares y el estado de cada uno.</summary>
    /// <param name="id">Número del recurso.</param>
    /// <response code="200">El recurso y sus ejemplares.</response>
    /// <response code="400">El recurso no existe o fue dado de baja.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RespuestaApi<RecursoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        return Responder(await _recursos.ObtenerPorIdAsync(UsuarioActualId, id));
    }

    /// <summary>Registra un recurso nuevo con sus ejemplares. Todos quedan Disponibles.</summary>
    /// <response code="200">Recurso registrado, con los códigos de sus ejemplares.</response>
    /// <response code="400">Datos inválidos, o ya existe un recurso con ese código.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<RecursoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromBody] CrearRecursoRequest solicitud)
    {
        var resultado = await _recursos.GuardarAsync(new GuardarRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Codigo = solicitud.Codigo,
            Titulo = solicitud.Titulo,
            Autor = solicitud.Autor,
            Categoria = solicitud.Categoria,
            CantidadEjemplares = solicitud.CantidadEjemplares
        });

        return Responder(resultado);
    }

    /// <summary>Actualiza el título, el autor y la categoría de un recurso. El código no se cambia.</summary>
    /// <param name="id">Número del recurso.</param>
    /// <response code="200">Recurso actualizado.</response>
    /// <response code="400">Datos inválidos, o el recurso no existe.</response>
    /// <response code="403">Solo el administrador y el personal bibliotecario.</response>
    [Authorize(Roles = Roles.AdminPersonal)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RespuestaApi<RecursoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Put(int id, [FromBody] ActualizarRecursoRequest solicitud)
    {
        var resultado = await _recursos.ActualizarAsync(new ActualizarRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = id,
            Titulo = solicitud.Titulo,
            Autor = solicitud.Autor,
            Categoria = solicitud.Categoria
        });

        return Responder(resultado);
    }

    /// <summary>Da de baja un recurso: deja de aparecer en el catálogo, pero su historial se conserva.</summary>
    /// <param name="id">Número del recurso.</param>
    /// <response code="200">Recurso dado de baja.</response>
    /// <response code="400">El recurso no existe, ya estaba de baja o tiene préstamos activos.</response>
    /// <response code="403">Solo el administrador.</response>
    [Authorize(Roles = Roles.Administrador)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _recursos.DarDeBajaAsync(new BajaRecursoDto
        {
            UsuarioAccionId = UsuarioActualId,
            Id = id
        });

        return Responder(resultado);
    }

    /// <summary>Muestra el historial de cambios de estado de los ejemplares de un recurso, del más reciente al más antiguo.</summary>
    /// <param name="id">Número del recurso.</param>
    /// <response code="200">Lista de cambios, con quién los hizo y el motivo.</response>
    /// <response code="400">El recurso no existe.</response>
    /// <response code="403">Solo el administrador, el personal bibliotecario y el auditor.</response>
    [Authorize(Roles = Roles.AdminPersonalAuditor)]
    [HttpGet("{id:int}/Historial")]
    [ProducesResponseType(typeof(RespuestaApi<List<HistorialEjemplarDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistorial(int id)
    {
        return Responder(await _recursos.ObtenerHistorialAsync(UsuarioActualId, id));
    }
}