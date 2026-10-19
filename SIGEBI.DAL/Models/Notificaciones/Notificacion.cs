using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.DAL.Models.Notificaciones;

// Registro que solo se agrega (salvo Leida): no hereda AuditEntity.
public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int? PrestamoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaEnvio { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Prestamo? Prestamo { get; set; }
}
