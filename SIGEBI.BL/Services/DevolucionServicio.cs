using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Devolucion;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class DevolucionServicio : IDevolucionServicio
{
    private const int LargoMaximoObservacion = 300;

    private readonly IPrestamoRepositorio _prestamos;
    private readonly IEjemplarRepositorio _ejemplares;
    private readonly IRecursoRepositorio _recursos;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IHistorialEstadoEjemplarRepositorio _historial;
    private readonly IPenalizacionRepositorio _penalizaciones;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IGeneradorNotificaciones _notificaciones;
    private readonly IUnidadDeTrabajo _unidad;

    public DevolucionServicio(
        IPrestamoRepositorio prestamos,
        IEjemplarRepositorio ejemplares,
        IRecursoRepositorio recursos,
        IUsuarioRepositorio usuarios,
        IHistorialEstadoEjemplarRepositorio historial,
        IPenalizacionRepositorio penalizaciones,
        IRegistradorAuditoria auditoria,
        IGeneradorNotificaciones notificaciones,
        IUnidadDeTrabajo unidad)
    {
        _prestamos = prestamos;
        _ejemplares = ejemplares;
        _recursos = recursos;
        _usuarios = usuarios;
        _historial = historial;
        _penalizaciones = penalizaciones;
        _auditoria = auditoria;
        _notificaciones = notificaciones;
        _unidad = unidad;
    }

    // ---------- Registrar devolución (CU-DEV-01 y CU-DEV-02) ----------

    public async Task<ResultadoOperacion> RegistrarAsync(RegistrarDevolucionDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "RegistrarDevolucion", motivoActor);
        }

        if (string.IsNullOrWhiteSpace(dto.CodigoEjemplar))
        {
            return await RechazarAccionAsync(actor!.Id, "RegistrarDevolucion", "Escribe el código del ejemplar que se devuelve.");
        }

        var observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();

        if (observacion != null && observacion.Length > LargoMaximoObservacion)
        {
            return await RechazarAccionAsync(actor!.Id, "RegistrarDevolucion", $"La observación no puede tener más de {LargoMaximoObservacion} caracteres.");
        }

        var ejemplar = await _ejemplares.ObtenerPorCodigoAsync(dto.CodigoEjemplar.Trim());

        if (ejemplar == null)
        {
            return await RechazarAccionAsync(actor!.Id, "RegistrarDevolucion", "No existe un ejemplar con ese código.");
        }

        var prestamo = await _prestamos.ObtenerActivoPorEjemplarAsync(ejemplar.Id);

        if (prestamo == null)
        {
            return await RechazarAccionAsync(actor!.Id, "RegistrarDevolucion", "Ese ejemplar no tiene un préstamo activo.");
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(prestamo.UsuarioId);
        var recurso = await _recursos.ObtenerPorIdAsync(ejemplar.RecursoId);
        var titulo = recurso?.Titulo ?? ejemplar.CodigoEjemplar;

        // Si llega después de la fecha límite se genera la penalización
        var ahora = DateTime.Now;
        var diasRetraso = CalcularRetraso(prestamo.FechaLimite, ahora);
        var estadoAnterior = ejemplar.Estado;
        var estadoNuevo = dto.Danado ? EstadosEjemplar.FueraDeServicio : EstadosEjemplar.Disponible;

        prestamo.Estado = EstadosPrestamo.Devuelto;
        prestamo.FechaDevolucion = ahora;
        prestamo.RecibidoPorId = actor!.Id;
        prestamo.ObservacionDevolucion = observacion;
        prestamo.UsuarioModificacion = actor.Id;

        ejemplar.Estado = estadoNuevo;
        ejemplar.UsuarioModificacion = actor.Id;

        Penalizacion? penalizacion = null;

        if (diasRetraso > 0)
        {
            penalizacion = new Penalizacion
            {
                UsuarioId = prestamo.UsuarioId,
                PrestamoId = prestamo.Id,
                DiasRetraso = diasRetraso,
                Descripcion = Texto.Recortar($"Devolución con {diasRetraso} día(s) de retraso de \"{titulo}\".", 200),
                Estado = EstadosPenalizacion.Activa,
                UsuarioCreacion = actor.Id
            };
        }

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _prestamos.ActualizarAsync(prestamo);

            if (!guardado.Exito)
            {
                return guardado;
            }

            guardado = await _ejemplares.ActualizarAsync(ejemplar);

            if (!guardado.Exito)
            {
                return guardado;
            }

            guardado = await _historial.GuardarAsync(new HistorialEstadoEjemplar
            {
                EjemplarId = ejemplar.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                Fecha = ahora,
                UsuarioId = actor.Id,
                Motivo = Texto.Recortar(dto.Danado ? $"Devolución del préstamo {prestamo.Id} con daños" : $"Devolución del préstamo {prestamo.Id}", 200)
            });

            if (!guardado.Exito)
            {
                return guardado;
            }

            if (penalizacion != null)
            {
                guardado = await _penalizaciones.GuardarAsync(penalizacion);

                if (!guardado.Exito)
                {
                    return guardado;
                }

                var avisoPenalizacion = await _notificaciones.CrearAsync(prestamo.UsuarioId, prestamo.Id, TiposEvento.PenalizacionGenerada,
                    $"Se generó una penalización por devolver \"{titulo}\" con {diasRetraso} día(s) de retraso. Mientras esté activa no podrás pedir préstamos.");

                if (!avisoPenalizacion.Exito)
                {
                    return avisoPenalizacion;
                }

                var auditadoPenalizacion = await _auditoria.RegistrarAsync(actor.Id, Modulos.Penalizaciones, "GenerarPenalizacion",
                    ResultadosAuditoria.Exitoso, $"Préstamo {prestamo.Id}, {diasRetraso} día(s) de retraso");

                if (!auditadoPenalizacion.Exito)
                {
                    return auditadoPenalizacion;
                }
            }

            var aviso = await _notificaciones.CrearAsync(prestamo.UsuarioId, prestamo.Id, TiposEvento.PrestamoDevuelto,
                $"Registramos la devolución de \"{titulo}\"." + (penalizacion != null ? " Se generó una penalización por retraso." : string.Empty));

            if (!aviso.Exito)
            {
                return aviso;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Devoluciones, "RegistrarDevolucion",
                ResultadosAuditoria.Exitoso, $"Préstamo {prestamo.Id}, ejemplar {ejemplar.CodigoEjemplar}, retraso {diasRetraso} día(s)");

            if (!auditado.Exito)
            {
                return auditado;
            }

            var resultado = ADto(prestamo, usuario, titulo, ejemplar.CodigoEjemplar);
            resultado.PenalizacionGenerada = penalizacion != null;
            resultado.EstadoEjemplar = estadoNuevo;

            return Correcto(penalizacion != null
                ? "La devolución se registró y se generó una penalización por retraso."
                : "La devolución se registró correctamente.", resultado);
        });
    }

    // ---------- Historial de devoluciones (CU-DEV-03) ----------

    public async Task<ResultadoOperacion> ObtenerHistorialPorUsuarioAsync(int usuarioAccionId, int usuarioId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId,
            TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);

        if (usuario == null)
        {
            return Rechazo("El usuario no existe.");
        }

        var prestamos = await _prestamos.ObtenerPorUsuarioAsync(usuarioId, EstadosPrestamo.Devuelto);

        var lista = prestamos
            .Select(p => ADto(p, usuario, p.Ejemplar?.Recurso?.Titulo, p.Ejemplar?.CodigoEjemplar))
            .ToList();

        return Correcto("Consulta realizada.", lista);
    }

    public async Task<ResultadoOperacion> ObtenerHistorialPorRecursoAsync(int usuarioAccionId, int recursoId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId,
            TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var recurso = await _recursos.ObtenerPorIdAsync(recursoId);

        if (recurso == null)
        {
            return Rechazo("El recurso no existe.");
        }

        var prestamos = await _prestamos.ObtenerPorRecursoAsync(recursoId);

        var lista = prestamos
            .Where(p => p.Estado == EstadosPrestamo.Devuelto)
            .Select(p => ADto(p, p.Usuario, recurso.Titulo, p.Ejemplar?.CodigoEjemplar))
            .ToList();

        return Correcto("Consulta realizada.", lista);
    }

    // ---------- Apoyo ----------

    // Días de retraso: 0 si llegó a tiempo. Si llegó tarde, mínimo 1.
    private static int CalcularRetraso(DateTime? fechaLimite, DateTime fechaDevolucion)
    {
        if (fechaLimite == null || fechaDevolucion <= fechaLimite)
        {
            return 0;
        }

        return Math.Max(1, (fechaDevolucion.Date - fechaLimite.Value.Date).Days);
    }

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
        await _auditoria.RegistrarAsync(usuarioId, Modulos.Devoluciones, accion, ResultadosAuditoria.Rechazado, motivo);

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

    private static DevolucionDto ADto(Prestamo prestamo, Usuario? usuario, string? titulo, string? codigoEjemplar)
    {
        return new DevolucionDto
        {
            PrestamoId = prestamo.Id,
            UsuarioId = prestamo.UsuarioId,
            NombreUsuario = usuario == null ? null : $"{usuario.Nombre} {usuario.Apellido}",
            EjemplarId = prestamo.EjemplarId,
            CodigoEjemplar = codigoEjemplar,
            TituloRecurso = titulo,
            FechaLimite = prestamo.FechaLimite,
            FechaDevolucion = prestamo.FechaDevolucion,
            Observacion = prestamo.ObservacionDevolucion,
            DiasRetraso = prestamo.FechaDevolucion == null ? 0 : CalcularRetraso(prestamo.FechaLimite, prestamo.FechaDevolucion.Value)
        };
    }
}