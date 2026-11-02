using System.Text;
using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Auditoria;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Auditoria;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.Persistence.Interfaces.Auditoria;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class AuditoriaServicio : IAuditoriaServicio
{
    private static readonly string[] ModulosValidos =
    {
        Modulos.Usuarios,
        Modulos.Catalogo,
        Modulos.Prestamos,
        Modulos.Devoluciones,
        Modulos.Penalizaciones,
        Modulos.Notificaciones,
        Modulos.Reportes
    };

    private readonly IRegistroAuditoriaRepositorio _registros;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRegistradorAuditoria _auditoria;

    public AuditoriaServicio(
        IRegistroAuditoriaRepositorio registros,
        IUsuarioRepositorio usuarios,
        IRegistradorAuditoria auditoria)
    {
        _registros = registros;
        _usuarios = usuarios;
        _auditoria = auditoria;
    }

    // ---------- Consultar el registro de auditoría (CU-REP-04) ----------

    // Solo lectura: nadie modifica ni borra el registro desde aquí
    public async Task<ResultadoOperacion> ConsultarAsync(FiltroAuditoriaDto filtro)
    {
        var (registros, error, _) = await BuscarAsync(filtro);

        if (error != null)
        {
            return Rechazo(error);
        }

        return Correcto(registros!.Count == 0 ? "No se encontraron registros." : "Consulta realizada.", registros.Select(ADto).ToList());
    }

    public async Task<ResultadoOperacion> ExportarCsvAsync(FiltroAuditoriaDto filtro)
    {
        var (registros, error, actorId) = await BuscarAsync(filtro);

        if (error != null)
        {
            return Rechazo(error);
        }

        var lineas = new List<string> { "Fecha,Usuario,Modulo,Accion,Resultado,Detalle" };

        foreach (var registro in registros!.Select(ADto))
        {
            lineas.Add(string.Join(",", new[]
            {
                Campo(registro.Fecha.ToString("yyyy-MM-dd HH:mm:ss")),
                Campo(registro.Usuario ?? "Sistema"),
                Campo(registro.Modulo),
                Campo(registro.TipoAccion),
                Campo(registro.Resultado),
                Campo(registro.Detalle)
            }));
        }

        // Exportar información también queda registrado
        await _auditoria.RegistrarAsync(actorId, Modulos.Reportes, "ExportarAuditoria",
            ResultadosAuditoria.Exitoso, $"{registros!.Count} registro(s) exportados");

        return Correcto($"Se exportaron {registros.Count} registro(s).", string.Join("\r\n", lineas));
    }

    // ---------- Apoyo ----------

    // Comprueba permisos y filtros, y trae los registros
    private async Task<(List<RegistroAuditoria>? Registros, string? Error, int? ActorId)> BuscarAsync(FiltroAuditoriaDto filtro)
    {
        var (actor, motivo) = await ObtenerActorAsync(filtro.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return (null, motivo, actor?.Id);
        }

        var modulo = string.IsNullOrWhiteSpace(filtro.Modulo) ? null : filtro.Modulo.Trim();

        if (modulo != null && !ModulosValidos.Contains(modulo))
        {
            return (null, "El módulo indicado no es válido.", actor!.Id);
        }

        if (filtro.Desde != null && filtro.Hasta != null && filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
        {
            return (null, "La fecha inicial no puede ser posterior a la final.", actor!.Id);
        }

        var accion = string.IsNullOrWhiteSpace(filtro.TipoAccion) ? null : filtro.TipoAccion.Trim();
        var registros = await _registros.ConsultarAsync(filtro.UsuarioId, modulo, accion, filtro.Desde, filtro.Hasta);

        return (registros, null, actor!.Id);
    }

    // Texto seguro para CSV: comillas cuando hace falta, y sin que una hoja de cálculo lo tome como fórmula
    private static string Campo(string? valor)
    {
        var texto = valor ?? string.Empty;

        if (texto.Length > 0 && "=+-@".Contains(texto[0]))
        {
            texto = "'" + texto;
        }

        return texto.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + texto.Replace("\"", "\"\"") + "\"" : texto;
    }

    private static ResultadoOperacion Rechazo(string mensaje)
    {
        return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }

    private static ResultadoOperacion Correcto(string mensaje, object? datos = null)
    {
        return new ResultadoOperacion { Exito = true, Mensaje = mensaje, Datos = datos };
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

    private static AuditoriaDto ADto(RegistroAuditoria registro)
    {
        return new AuditoriaDto
        {
            Id = registro.Id,
            Fecha = registro.Fecha,
            Usuario = registro.Usuario == null ? null : $"{registro.Usuario.Nombre} {registro.Usuario.Apellido}",
            Modulo = registro.Modulo,
            TipoAccion = registro.TipoAccion,
            Resultado = registro.Resultado,
            Detalle = registro.Detalle
        };
    }
}