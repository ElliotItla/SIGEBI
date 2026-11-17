namespace SIGEBI.Api.Requests.Auditoria;

// Todos los filtros son opcionales. Sin ninguno, se exporta todo el registro.
public class ExportarAuditoriaRequest
{
    /// <summary>Número del usuario que realizó las acciones.</summary>
    public int? UsuarioId { get; set; }

    /// <summary>Usuarios, Catalogo, Prestamos, Devoluciones, Penalizaciones, Notificaciones o Reportes.</summary>
    /// <example>Prestamos</example>
    public string? Modulo { get; set; }

    /// <summary>Nombre de la acción, por ejemplo AprobarPrestamo.</summary>
    public string? TipoAccion { get; set; }

    /// <summary>Fecha inicial (yyyy-MM-dd).</summary>
    public DateTime? Desde { get; set; }

    /// <summary>Fecha final (yyyy-MM-dd).</summary>
    public DateTime? Hasta { get; set; }
}