using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Notificacion;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Interfaces.Configuracion;
using SIGEBI.Persistence.Interfaces.Notificaciones;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class NotificacionServicio : INotificacionServicio
{
    private const string ClaveDiasAviso = "DiasAvisoVencimiento";
    private const int DiasAvisoPorDefecto = 2;

    private static readonly string[] TiposValidos =
    {
        TiposEvento.ProximoVencimiento,
        TiposEvento.PenalizacionGenerada,
        TiposEvento.PrestamoSolicitado,
        TiposEvento.PrestamoConfirmado,
        TiposEvento.PrestamoDevuelto,
        TiposEvento.PenalizacionResuelta
    };

    private readonly INotificacionRepositorio _notificaciones;
    private readonly IPrestamoRepositorio _prestamos;
    private readonly IParametroSistemaRepositorio _parametros;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IGeneradorNotificaciones _generador;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IUnidadDeTrabajo _unidad;

    public NotificacionServicio(
        INotificacionRepositorio notificaciones,
        IPrestamoRepositorio prestamos,
        IParametroSistemaRepositorio parametros,
        IUsuarioRepositorio usuarios,
        IGeneradorNotificaciones generador,
        IRegistradorAuditoria auditoria,
        IUnidadDeTrabajo unidad)
    {
        _notificaciones = notificaciones;
        _prestamos = prestamos;
        _parametros = parametros;
        _usuarios = usuarios;
        _generador = generador;
        _auditoria = auditoria;
        _unidad = unidad;
    }

    // ---------- Aviso de próximo vencimiento (CU-NOT-01) ----------

    // Lo ejecuta el sistema de forma programada: no lo pide ningún usuario.
    // Un préstamo ya avisado no se vuelve a avisar.
    public async Task<ResultadoOperacion> NotificarProximosVencimientosAsync()
    {
        var dias = await LeerParametroAsync(ClaveDiasAviso, DiasAvisoPorDefecto);
        var prestamos = await _prestamos.ObtenerProximosAVencerAsync(dias);

        if (prestamos.Count == 0)
        {
            return Correcto("No hay préstamos por vencer que avisar.", 0);
        }

        var hoy = DateTime.Now.Date;

        return await _unidad.EjecutarAsync(async () =>
        {
            foreach (var prestamo in prestamos)
            {
                var limite = prestamo.FechaLimite ?? DateTime.Now;
                var faltan = (limite.Date - hoy).Days;
                var cuando = faltan <= 0 ? "hoy" : faltan == 1 ? "mañana" : $"en {faltan} días";
                var titulo = prestamo.Ejemplar?.Recurso?.Titulo ?? prestamo.Ejemplar?.CodigoEjemplar ?? "el recurso";

                var aviso = await _generador.CrearAsync(prestamo.UsuarioId, prestamo.Id, TiposEvento.ProximoVencimiento,
                    $"Tu préstamo de \"{titulo}\" vence {cuando} ({limite:dd/MM/yyyy}). Devuélvelo a tiempo para evitar una penalización.");

                if (!aviso.Exito)
                {
                    return aviso;
                }
            }

            var auditado = await _auditoria.RegistrarAsync(null, Modulos.Notificaciones, "NotificarVencimientos",
                ResultadosAuditoria.Exitoso, $"{prestamos.Count} aviso(s) de vencimiento");

            return auditado.Exito ? Correcto($"Se enviaron {prestamos.Count} aviso(s) de vencimiento.", prestamos.Count) : auditado;
        });
    }

    // ---------- Mis notificaciones ----------

    public async Task<ResultadoOperacion> ObtenerMisNotificacionesAsync(int usuarioId, bool soloNoLeidas)
    {
        var motivo = await ComprobarUsuarioAsync(usuarioId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var notificaciones = await _notificaciones.ObtenerPorUsuarioAsync(usuarioId, soloNoLeidas);

        return Correcto("Consulta realizada.", notificaciones.Select(ADto).ToList());
    }

    public async Task<ResultadoOperacion> ContarNoLeidasAsync(int usuarioId)
    {
        var motivo = await ComprobarUsuarioAsync(usuarioId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var cantidad = await _notificaciones.ContarNoLeidasAsync(usuarioId);

        return Correcto("Consulta realizada.", cantidad);
    }

    public async Task<ResultadoOperacion> MarcarComoLeidasAsync(int usuarioId)
    {
        var motivo = await ComprobarUsuarioAsync(usuarioId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var resultado = await _notificaciones.MarcarComoLeidasAsync(usuarioId);

        return resultado.Exito ? Correcto("Tus notificaciones quedaron como leídas.") : resultado;
    }

    // ---------- Registro de notificaciones enviadas (CU-NOT-05) ----------

    public async Task<ResultadoOperacion> ObtenerRegistroAsync(FiltroNotificacionDto filtro)
    {
        var (_, motivo) = await ObtenerActorAsync(filtro.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var tipo = string.IsNullOrWhiteSpace(filtro.TipoEvento) ? null : filtro.TipoEvento.Trim();

        if (tipo != null && !TiposValidos.Contains(tipo))
        {
            return Rechazo("El tipo de evento no es válido.");
        }

        if (filtro.Desde != null && filtro.Hasta != null && filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
        {
            return Rechazo("La fecha inicial no puede ser posterior a la final.");
        }

        var registro = await _notificaciones.ObtenerRegistroAsync(filtro.UsuarioId, tipo, filtro.Desde, filtro.Hasta);

        return Correcto(registro.Count == 0 ? "No se encontraron notificaciones." : "Consulta realizada.", registro.Select(ADto).ToList());
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

    private async Task<int> LeerParametroAsync(string clave, int valorPorDefecto)
    {
        var parametro = await _parametros.ObtenerPorClaveAsync(clave);

        return parametro != null && parametro.Valor > 0 ? parametro.Valor : valorPorDefecto;
    }

    // Quien consulta sus notificaciones debe ser un usuario activo
    private async Task<string?> ComprobarUsuarioAsync(int usuarioId)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);

        return usuario == null || !usuario.Estatus ? "El usuario no existe o está inactivo." : null;
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

    private static NotificacionDto ADto(Notificacion notificacion)
    {
        return new NotificacionDto
        {
            Id = notificacion.Id,
            UsuarioId = notificacion.UsuarioId,
            NombreUsuario = notificacion.Usuario == null ? null : $"{notificacion.Usuario.Nombre} {notificacion.Usuario.Apellido}",
            PrestamoId = notificacion.PrestamoId,
            TipoEvento = notificacion.TipoEvento,
            Mensaje = notificacion.Mensaje,
            FechaEnvio = notificacion.FechaEnvio,
            Leida = notificacion.Leida
        };
    }
}