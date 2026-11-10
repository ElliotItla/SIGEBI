using SIGEBI.BL.Dtos.Notificacion;

namespace SIGEBI.Web.Models.Notificaciones;

public class NotificacionesIndexViewModel
{
    public bool SoloNoLeidas { get; set; }
    public int NoLeidas { get; set; }
    public List<NotificacionDto> Notificaciones { get; set; } = new();
}