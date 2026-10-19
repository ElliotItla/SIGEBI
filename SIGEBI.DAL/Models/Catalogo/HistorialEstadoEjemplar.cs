using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.DAL.Models.Catalogo;

// Registro que solo se agrega: no se modifica ni hereda AuditEntity.
public class HistorialEstadoEjemplar
{
    public int Id { get; set; }
    public int EjemplarId { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int? UsuarioId { get; set; }
    public string? Motivo { get; set; }

    public Ejemplar Ejemplar { get; set; } = null!;
    public Usuario? Usuario { get; set; }
}
