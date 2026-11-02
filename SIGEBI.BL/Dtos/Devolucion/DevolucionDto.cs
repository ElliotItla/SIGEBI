namespace SIGEBI.BL.Dtos.Devolucion;

public class DevolucionDto
{
    public int PrestamoId { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public int EjemplarId { get; set; }
    public string? CodigoEjemplar { get; set; }
    public string? TituloRecurso { get; set; }
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string? Observacion { get; set; }

    // Días de retraso. Es 0 cuando se devolvió a tiempo.
    public int DiasRetraso { get; set; }

    // Solo se llena al registrar la devolución
    public bool PenalizacionGenerada { get; set; }
    public string? EstadoEjemplar { get; set; }
}