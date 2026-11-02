using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Prestamo;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Configuracion;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Configuracion;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class PrestamoServicio : IPrestamoServicio
{
    private const int LargoMaximoMotivo = 200;

    private static readonly string[] EstadosValidos =
    {
        EstadosPrestamo.Pendiente,
        EstadosPrestamo.Rechazado,
        EstadosPrestamo.Activo,
        EstadosPrestamo.Devuelto
    };

    private readonly IPrestamoRepositorio _prestamos;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRecursoRepositorio _recursos;
    private readonly IEjemplarRepositorio _ejemplares;
    private readonly IHistorialEstadoEjemplarRepositorio _historial;
    private readonly IPoliticaPrestamoRepositorio _politicas;
    private readonly IPenalizacionRepositorio _penalizaciones;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IGeneradorNotificaciones _notificaciones;
    private readonly IUnidadDeTrabajo _unidad;

    public PrestamoServicio(
        IPrestamoRepositorio prestamos,
        IUsuarioRepositorio usuarios,
        IRecursoRepositorio recursos,
        IEjemplarRepositorio ejemplares,
        IHistorialEstadoEjemplarRepositorio historial,
        IPoliticaPrestamoRepositorio politicas,
        IPenalizacionRepositorio penalizaciones,
        IRegistradorAuditoria auditoria,
        IGeneradorNotificaciones notificaciones,
        IUnidadDeTrabajo unidad)
    {
        _prestamos = prestamos;
        _usuarios = usuarios;
        _recursos = recursos;
        _ejemplares = ejemplares;
        _historial = historial;
        _politicas = politicas;
        _penalizaciones = penalizaciones;
        _auditoria = auditoria;
        _notificaciones = notificaciones;
        _unidad = unidad;
    }

    // ---------- Solicitar (CU-PRE-01) ----------

    public async Task<ResultadoOperacion> SolicitarAsync(SolicitarPrestamoDto dto)
    {
        if (dto.UsuarioAccionId == null)
        {
            return Rechazo("No se indicó quién solicita el préstamo.");
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(dto.UsuarioAccionId.Value);

        if (usuario == null)
        {
            return Rechazo("No se encontró al usuario que solicita el préstamo.");
        }

        var (motivo, _) = await ComprobarHabilitacionAsync(usuario);

        if (motivo != null)
        {
            return await RechazarAccionAsync(usuario.Id, "SolicitarPrestamo", motivo);
        }

        var recurso = await _recursos.ObtenerPorIdAsync(dto.RecursoId);

        if (recurso == null || !recurso.Estatus)
        {
            return await RechazarAccionAsync(usuario.Id, "SolicitarPrestamo", "El recurso no existe.");
        }

        var ejemplar = await _ejemplares.ObtenerDisponibleDeRecursoAsync(recurso.Id);

        if (ejemplar == null)
        {
            return await RechazarAccionAsync(usuario.Id, "SolicitarPrestamo", "No hay ejemplares disponibles de este recurso.");
        }

        var personal = await _usuarios.ObtenerPorFiltroAsync(TiposUsuario.PersonalBibliotecario, true);

        var prestamo = new Prestamo
        {
            UsuarioId = usuario.Id,
            EjemplarId = ejemplar.Id,
            Estado = EstadosPrestamo.Pendiente,
            UsuarioCreacion = usuario.Id
        };

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _prestamos.GuardarAsync(prestamo);

            if (!guardado.Exito)
            {
                return guardado;
            }

            // Confirmación para quien pidió el préstamo
            var aviso = await _notificaciones.CrearAsync(usuario.Id, prestamo.Id, TiposEvento.PrestamoSolicitado,
                $"Recibimos tu solicitud de préstamo de \"{recurso.Titulo}\". El personal la revisará pronto.");

            if (!aviso.Exito)
            {
                return aviso;
            }

            // Aviso para el personal bibliotecario
            foreach (var persona in personal)
            {
                aviso = await _notificaciones.CrearAsync(persona.Id, prestamo.Id, TiposEvento.PrestamoSolicitado,
                    $"Nueva solicitud de préstamo de {usuario.Nombre} {usuario.Apellido} para \"{recurso.Titulo}\".");

                if (!aviso.Exito)
                {
                    return aviso;
                }
            }

            var auditado = await _auditoria.RegistrarAsync(usuario.Id, Modulos.Prestamos, "SolicitarPrestamo",
                ResultadosAuditoria.Exitoso, $"Préstamo {prestamo.Id}, ejemplar {ejemplar.CodigoEjemplar}");

            return auditado.Exito
                ? Correcto("La solicitud se registró y quedó pendiente de aprobación.", ADto(prestamo, usuario, ejemplar, recurso))
                : auditado;
        });
    }

    // ---------- Aprobar y registrar (CU-PRE-02) ----------

    public async Task<ResultadoOperacion> AprobarAsync(AprobarPrestamoDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "AprobarPrestamo", motivoActor);
        }

        var prestamo = await _prestamos.ObtenerPorIdAsync(dto.PrestamoId);

        if (prestamo == null)
        {
            return await RechazarAccionAsync(actor!.Id, "AprobarPrestamo", "La solicitud no existe.");
        }

        if (prestamo.Estado != EstadosPrestamo.Pendiente)
        {
            return await RechazarAccionAsync(actor!.Id, "AprobarPrestamo", $"La solicitud ya fue procesada (estado: {prestamo.Estado}).");
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(prestamo.UsuarioId);

        if (usuario == null)
        {
            return await RechazarAccionAsync(actor!.Id, "AprobarPrestamo", "El usuario de la solicitud no existe.");
        }

        // Se vuelven a revisar las condiciones: pudieron cambiar desde que se hizo la solicitud
        var (motivo, politica) = await ComprobarHabilitacionAsync(usuario);

        if (motivo != null)
        {
            return await RechazarAccionAsync(actor!.Id, "AprobarPrestamo", $"No se puede aprobar: {motivo}");
        }

        var ejemplar = await _ejemplares.ObtenerPorIdAsync(prestamo.EjemplarId);

        if (ejemplar == null || !ejemplar.Estatus || ejemplar.Estado != EstadosEjemplar.Disponible)
        {
            return await RechazarAccionAsync(actor!.Id, "AprobarPrestamo", "No se puede aprobar: el ejemplar ya no está disponible.");
        }

        var recurso = await _recursos.ObtenerPorIdAsync(ejemplar.RecursoId);

        // El plazo vence al final del último día
        var ahora = DateTime.Now;
        var limite = ahora.Date.AddDays(politica!.DiasPrestamo + 1).AddMinutes(-1);
        var estadoAnterior = ejemplar.Estado;

        prestamo.Estado = EstadosPrestamo.Activo;
        prestamo.FechaInicio = ahora;
        prestamo.FechaLimite = limite;
        prestamo.AprobadoPorId = actor!.Id;
        prestamo.UsuarioModificacion = actor.Id;

        ejemplar.Estado = EstadosEjemplar.Prestado;
        ejemplar.UsuarioModificacion = actor.Id;

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
                EstadoNuevo = EstadosEjemplar.Prestado,
                Fecha = ahora,
                UsuarioId = actor.Id,
                Motivo = $"Préstamo {prestamo.Id}"
            });

            if (!guardado.Exito)
            {
                return guardado;
            }

            var titulo = recurso?.Titulo ?? ejemplar.CodigoEjemplar;
            var aviso = await _notificaciones.CrearAsync(usuario.Id, prestamo.Id, TiposEvento.PrestamoConfirmado,
                $"Tu préstamo de \"{titulo}\" fue aprobado. Devuélvelo antes del {limite:dd/MM/yyyy}.");

            if (!aviso.Exito)
            {
                return aviso;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Prestamos, "AprobarPrestamo",
                ResultadosAuditoria.Exitoso, $"Préstamo {prestamo.Id}, ejemplar {ejemplar.CodigoEjemplar}, vence {limite:dd/MM/yyyy}");

            return auditado.Exito
                ? Correcto("El préstamo se aprobó y quedó registrado.", ADto(prestamo, usuario, ejemplar, recurso))
                : auditado;
        });
    }

    // ---------- Rechazar ----------

    public async Task<ResultadoOperacion> RechazarAsync(RechazarPrestamoDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "RechazarPrestamo", motivoActor);
        }

        var prestamo = await _prestamos.ObtenerPorIdAsync(dto.PrestamoId);

        if (prestamo == null)
        {
            return await RechazarAccionAsync(actor!.Id, "RechazarPrestamo", "La solicitud no existe.");
        }

        if (prestamo.Estado != EstadosPrestamo.Pendiente)
        {
            return await RechazarAccionAsync(actor!.Id, "RechazarPrestamo", $"La solicitud ya fue procesada (estado: {prestamo.Estado}).");
        }

        if (string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length > LargoMaximoMotivo)
        {
            return await RechazarAccionAsync(actor!.Id, "RechazarPrestamo", $"El motivo es obligatorio y no puede tener más de {LargoMaximoMotivo} caracteres.");
        }

        var motivo = dto.Motivo.Trim();

        prestamo.Estado = EstadosPrestamo.Rechazado;
        prestamo.MotivoRechazo = motivo;
        prestamo.UsuarioModificacion = actor!.Id;

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _prestamos.ActualizarAsync(prestamo);

            if (!guardado.Exito)
            {
                return guardado;
            }

            // No hay un tipo de aviso propio para el rechazo: se usa el de la solicitud
            var aviso = await _notificaciones.CrearAsync(prestamo.UsuarioId, prestamo.Id, TiposEvento.PrestamoSolicitado,
                $"Tu solicitud de préstamo fue rechazada. Motivo: {motivo}");

            if (!aviso.Exito)
            {
                return aviso;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Prestamos, "RechazarPrestamo",
                ResultadosAuditoria.Exitoso, $"Préstamo {prestamo.Id}. Motivo: {motivo}");

            return auditado.Exito ? Correcto("La solicitud se rechazó.", ADto(prestamo)) : auditado;
        });
    }

    // ---------- Consultas (CU-PRE-03 y CU-PRE-04) ----------

    // Solicitudes que esperan aprobación
    public async Task<ResultadoOperacion> ObtenerPendientesAsync(int usuarioAccionId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var prestamos = await _prestamos.ObtenerPendientesAsync();

        return Correcto("Consulta realizada.", prestamos.Select(p => ADto(p)).ToList());
    }

    // Cada usuario ve sus propios préstamos. El personal, el administrador y el auditor ven los de cualquiera.
    public async Task<ResultadoOperacion> ObtenerPorUsuarioAsync(int usuarioAccionId, int usuarioId, string? estado)
    {
        var (actor, motivo) = await ObtenerActorAsync(usuarioAccionId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var puedeVerTodos = actor!.Tipo == TiposUsuario.Administrador
            || actor.Tipo == TiposUsuario.PersonalBibliotecario
            || actor.Tipo == TiposUsuario.Auditor;

        if (actor.Id != usuarioId && !puedeVerTodos)
        {
            return Rechazo("No tienes permiso para ver los préstamos de otro usuario.");
        }

        if (!string.IsNullOrWhiteSpace(estado) && !EstadosValidos.Contains(estado))
        {
            return Rechazo("El estado indicado no es válido.");
        }

        var prestamos = await _prestamos.ObtenerPorUsuarioAsync(usuarioId, string.IsNullOrWhiteSpace(estado) ? null : estado);

        return Correcto("Consulta realizada.", prestamos.Select(p => ADto(p)).ToList());
    }

    public async Task<ResultadoOperacion> ObtenerPorRecursoAsync(int usuarioAccionId, int recursoId)
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

        var lista = prestamos.Select(p =>
        {
            var dto = ADto(p);
            dto.RecursoId = recurso.Id;
            dto.TituloRecurso = recurso.Titulo;
            return dto;
        }).ToList();

        return Correcto("Consulta realizada.", lista);
    }

    // ---------- Apoyo ----------

    // Condiciones que debe cumplir quien pide un préstamo (RN-ACC y RN-PRE). Devuelve el motivo si no las cumple.
    private async Task<(string? Motivo, PoliticaPrestamo? Politica)> ComprobarHabilitacionAsync(Usuario usuario)
    {
        if (!usuario.Estatus)
        {
            return ("El usuario está inactivo.", null);
        }

        if (usuario.Tipo != TiposUsuario.Estudiante && usuario.Tipo != TiposUsuario.Docente)
        {
            return ("Solo los estudiantes y los docentes piden préstamos.", null);
        }

        if (await _penalizaciones.TieneActivaAsync(usuario.Id))
        {
            return ("El usuario tiene una penalización activa.", null);
        }

        if (await _prestamos.TieneVencidosAsync(usuario.Id))
        {
            return ("El usuario tiene un préstamo vencido sin devolver.", null);
        }

        var politica = await _politicas.ObtenerPorTipoUsuarioAsync(usuario.Tipo);

        if (politica == null)
        {
            return ("No hay una política de préstamo para este tipo de usuario.", null);
        }

        var activos = await _prestamos.ContarActivosPorUsuarioAsync(usuario.Id);

        if (activos >= politica.MaxPrestamos)
        {
            return ($"El usuario ya tiene {activos} préstamos activos y el máximo es {politica.MaxPrestamos}.", null);
        }

        return (null, politica);
    }

    private static ResultadoOperacion Rechazo(string mensaje)
    {
        return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }

    private static ResultadoOperacion Correcto(string mensaje, object? datos = null)
    {
        return new ResultadoOperacion { Exito = true, Mensaje = mensaje, Datos = datos };
    }

    // Deja el rechazo en la auditoría y devuelve el motivo al usuario
    private async Task<ResultadoOperacion> RechazarAccionAsync(int? usuarioId, string accion, string motivo)
    {
        await _auditoria.RegistrarAsync(usuarioId, Modulos.Prestamos, accion, ResultadosAuditoria.Rechazado, motivo);

        return Rechazo(motivo);
    }

    // Busca a quien realiza la acción. Si se indican tipos, solo esos tipos tienen permiso.
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

    // Para los resultados de una acción: se usan los datos que ya se tienen a mano
    private static PrestamoDto ADto(Prestamo prestamo, Usuario? usuario, Ejemplar? ejemplar, Recurso? recurso)
    {
        var dto = ADto(prestamo);

        if (usuario != null)
        {
            dto.NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}";
        }

        if (ejemplar != null)
        {
            dto.CodigoEjemplar = ejemplar.CodigoEjemplar;
            dto.RecursoId = ejemplar.RecursoId;
        }

        if (recurso != null)
        {
            dto.TituloRecurso = recurso.Titulo;
        }

        return dto;
    }

    // Para las listas: se usa lo que el repositorio haya cargado
    private static PrestamoDto ADto(Prestamo prestamo)
    {
        var ahora = DateTime.Now;
        var activo = prestamo.Estado == EstadosPrestamo.Activo && prestamo.FechaLimite != null;

        return new PrestamoDto
        {
            Id = prestamo.Id,
            UsuarioId = prestamo.UsuarioId,
            NombreUsuario = prestamo.Usuario == null ? null : $"{prestamo.Usuario.Nombre} {prestamo.Usuario.Apellido}",
            EjemplarId = prestamo.EjemplarId,
            CodigoEjemplar = prestamo.Ejemplar?.CodigoEjemplar,
            RecursoId = prestamo.Ejemplar?.RecursoId ?? 0,
            TituloRecurso = prestamo.Ejemplar?.Recurso?.Titulo,
            Estado = prestamo.Estado,
            FechaSolicitud = prestamo.FechaCreacion,
            FechaInicio = prestamo.FechaInicio,
            FechaLimite = prestamo.FechaLimite,
            FechaDevolucion = prestamo.FechaDevolucion,
            MotivoRechazo = prestamo.MotivoRechazo,
            Vencido = activo && prestamo.FechaLimite < ahora,
            DiasRestantes = activo ? (int)Math.Ceiling((prestamo.FechaLimite!.Value - ahora).TotalDays) : null
        };
    }
}