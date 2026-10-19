using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.DAL.Models.Prestamos;

namespace SIGEBI.DAL.Models.Seguridad;

public class Usuario : AuditEntity
{
    public string Identificador { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Penalizacion> Penalizaciones { get; set; } = new List<Penalizacion>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}
