using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Penalizacion;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class PenalizacionServicio : IPenalizacionServicio
{
    private const int LargoMaximoMotivo = 200;

    private readonly IPenalizacionRepositorio _penalizaciones;
    private readonly IPrestamoRepositorio _prestamos;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IGeneradorNotificaciones _notificaciones;
    private readonly IUnidadDeTrabajo _unidad;

    public PenalizacionServicio(
        IPenalizacionRepositorio penalizaciones,
        IPrestamoRepositorio prestamos,
        IUsuarioRepositorio usuarios,
        IRegistradorAuditoria auditoria,
        IGeneradorNotificaciones notificaciones,
        IUnidadDeTrabajo unidad)
    {
        _penalizaciones = penalizaciones;
        _prestamos = prestamos;
        _usuarios = usuarios;
        _auditoria = auditoria;
        _notificaciones = notificaciones;
        _unidad = unidad;
    }

    // ---------- Resolver (CU-PEN-03) ----------

    // Las penalizaciones se generan solas al registrar una devolución tardía (módulo de devoluciones)
    public async Task<ResultadoOperacion> ResolverAsync(ResolverPenalizacionDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "ResolverPenalizacion", motivoActor);
        }

        var penalizacion = await _penalizaciones.ObtenerPorIdAsync(dto.PenalizacionId);

        if (penalizacion == null)
        {
            return await RechazarAccionAsync(actor!.Id, "ResolverPenalizacion", "La penalización no existe.");
        }

        if (penalizacion.Estado == EstadosPenalizacion.Resuelta)
        {
            return await RechazarAccionAsync(actor!.Id, "ResolverPenalizacion", "La penalización ya estaba resuelta.");
        }

        if (string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length > LargoMaximoMotivo)
        {
            return await RechazarAccionAsync(actor!.Id, "ResolverPenalizacion", $"El motivo es obligatorio y no puede tener más de {LargoMaximoMotivo} caracteres.");
        }

        var motivo = dto.Motivo.Trim();
        var ahora = DateTime.Now;

        penalizacion.Estado = EstadosPenalizacion.Resuelta;
        penalizacion.FechaResolucion = ahora;
        penalizacion.ResueltaPorId = actor!.Id;
        penalizacion.MotivoResolucion = motivo;
        penalizacion.UsuarioModificacion = actor.Id;

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _penalizaciones.ActualizarAsync(penalizacion);

            if (!guardado.Exito)
            {
                return guardado;
            }

            // El aviso dice si el usuario ya puede pedir préstamos o si todavía le queda algo pendiente
            var sigueBloqueado = await _penalizaciones.TieneActivaAsync(penalizacion.UsuarioId)
                || await _prestamos.TieneVencidosAsync(penalizacion.UsuarioId);

            var mensaje = sigueBloqueado
                ? "Se resolvió una de tus penalizaciones, pero todavía tienes algo pendiente que te impide pedir préstamos."
                : "Tu penalización fue resuelta. Ya puedes volver a solicitar préstamos.";

            var aviso = await _notificaciones.CrearAsync(penalizacion.UsuarioId, penalizacion.PrestamoId, TiposEvento.PenalizacionResuelta, mensaje);

            if (!aviso.Exito)
            {
                return aviso;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Penalizaciones, "ResolverPenalizacion",
                ResultadosAuditoria.Exitoso, $"Penalización {penalizacion.Id}. Motivo: {motivo}");

            if (!auditado.Exito)
            {
                return auditado;
            }

            // La consulta por id no trae al usuario: se busca para devolver su nombre
            var resultado = ADto(penalizacion);
            var duenio = await _usuarios.ObtenerPorIdAsync(penalizacion.UsuarioId);

            if (duenio != null)
            {
                resultado.NombreUsuario = $"{duenio.Nombre} {duenio.Apellido}";
            }

            return Correcto("La penalización se resolvió.", resultado);
        });
    }

    // ---------- Consultas (CU-PEN-02) ----------

    // Todas las penalizaciones activas, para que el personal las resuelva
    public async Task<ResultadoOperacion> ObtenerActivasAsync(int usuarioAccionId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var penalizaciones = await _penalizaciones.ObtenerActivasAsync();

        return Correcto("Consulta realizada.", penalizaciones.Select(p => ADto(p)).ToList());
    }

    // Cada usuario ve las suyas. El personal, el administrador y el auditor ven las de cualquiera.
    public async Task<ResultadoOperacion> ObtenerPorUsuarioAsync(int usuarioAccionId, int usuarioId, bool soloActivas)
    {
        var (actor, motivo) = await ObtenerActorAsync(usuarioAccionId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var puedeVerTodas = actor!.Tipo == TiposUsuario.Administrador
            || actor.Tipo == TiposUsuario.PersonalBibliotecario
            || actor.Tipo == TiposUsuario.Auditor;

        if (actor.Id != usuarioId && !puedeVerTodas)
        {
            return Rechazo("No tienes permiso para ver las penalizaciones de otro usuario.");
        }

        var penalizaciones = await _penalizaciones.ObtenerPorUsuarioAsync(usuarioId, soloActivas);

        return Correcto("Consulta realizada.", penalizaciones.Select(p => ADto(p)).ToList());
    }

    // ---------- Apoyo ----------

    private static ResultadoOperacion Rechazo(string mensaje)
    {
        return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }

    private static ResultadoOperacion Correcto(string mensaje, object? datos = null)
    {
        return new ResultadoOperacion { Exito = true, Mensaje = mensaje, Datos = datos };
    }

    private async Task<ResultadoOperacion> RechazarAccionAsync(int? usuarioId, string accion, string motivo)
    {
        await _auditoria.RegistrarAsync(usuarioId, Modulos.Penalizaciones, accion, ResultadosAuditoria.Rechazado, motivo);

        return Rechazo(motivo);
    }

    private async Task<(Usuario? Actor, string? Motivo)> ObtenerActorAsync(int? usuarioAccionId, params string[] tiposPermitidos)
    {
        if (usuarioAccionId == null)
        {
            return (null, "No se indicó quién realiza la acción.");
        }

        var actor = await _usuarios.ObtenerPorIdAsync(usuarioAccionId.Value);

        if (actor == null || !actor.Estatus)
        {
            return (null, "El usuario que realiza la acción no existe o está inactivo.");
        }

        if (tiposPermitidos.Length > 0 && !tiposPermitidos.Contains(actor.Tipo))
        {
            return (actor, "No tienes permiso para realizar esta acción.");
        }

        return (actor, null);
    }

    // Usa lo que el repositorio haya cargado (usuario y recurso pueden venir vacíos)
    private static PenalizacionDto ADto(Penalizacion penalizacion)
    {
        return new PenalizacionDto
        {
            Id = penalizacion.Id,
            UsuarioId = penalizacion.UsuarioId,
            NombreUsuario = penalizacion.Usuario == null ? null : $"{penalizacion.Usuario.Nombre} {penalizacion.Usuario.Apellido}",
            PrestamoId = penalizacion.PrestamoId,
            TituloRecurso = penalizacion.Prestamo?.Ejemplar?.Recurso?.Titulo,
            DiasRetraso = penalizacion.DiasRetraso,
            Descripcion = penalizacion.Descripcion,
            Estado = penalizacion.Estado,
            FechaGeneracion = penalizacion.FechaCreacion,
            FechaResolucion = penalizacion.FechaResolucion,
            MotivoResolucion = penalizacion.MotivoResolucion
        };
    }
}