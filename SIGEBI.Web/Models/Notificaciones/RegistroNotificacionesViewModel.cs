using SIGEBI.BL.Dtos.Notificacion;

namespace SIGEBI.Web.Models.Notificaciones;

public class RegistroNotificacionesViewModel
{
    public int? UsuarioId { get; set; }
    public string? TipoEvento { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public List<NotificacionDto> Notificaciones { get; set; } = new();
}