namespace SIGEBI.BL.Dtos.Notificacion;

public class NotificacionDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public int? PrestamoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; }
    public bool Leida { get; set; }
}