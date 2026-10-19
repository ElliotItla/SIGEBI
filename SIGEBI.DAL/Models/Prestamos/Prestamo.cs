using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.DAL.Models.Prestamos;

// FechaCreacion es la fecha de la solicitud.
public class Prestamo : AuditEntity
{
    public int UsuarioId { get; set; }
    public int EjemplarId { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string? MotivoRechazo { get; set; }
    public int? AprobadoPorId { get; set; }
    public int? RecibidoPorId { get; set; }
    public string? ObservacionDevolucion { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Ejemplar Ejemplar { get; set; } = null!;
    public Usuario? AprobadoPor { get; set; }
    public Usuario? RecibidoPor { get; set; }
    public Penalizacion? Penalizacion { get; set; }
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}
