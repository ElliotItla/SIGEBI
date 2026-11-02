using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Notificacion;

// Filtros del registro de notificaciones enviadas. Todos son opcionales.
public class FiltroNotificacionDto : DtoBase
{
    public int? UsuarioId { get; set; }
    public string? TipoEvento { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}