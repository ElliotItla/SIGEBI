using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.DAL.Models.Prestamos;

// Un solo tipo de penalización (retraso). FechaCreacion es la fecha en que se generó.
public class Penalizacion : AuditEntity
{
    public int UsuarioId { get; set; }
    public int PrestamoId { get; set; }
    public int DiasRetraso { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activa";
    public DateTime? FechaResolucion { get; set; }
    public int? ResueltaPorId { get; set; }
    public string? MotivoResolucion { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Prestamo Prestamo { get; set; } = null!;
    public Usuario? ResueltaPor { get; set; }
}
