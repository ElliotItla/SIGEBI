using SIGEBI.DAL.Models.Prestamos;

namespace SIGEBI.DAL.Models.Catalogo;

public class Ejemplar : AuditEntity
{
    public int RecursoId { get; set; }
    public string CodigoEjemplar { get; set; } = string.Empty;
    public string Estado { get; set; } = "Disponible";

    public Recurso Recurso { get; set; } = null!;
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<HistorialEstadoEjemplar> Historial { get; set; } = new List<HistorialEstadoEjemplar>();
}
