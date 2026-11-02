namespace SIGEBI.BL.Dtos.Prestamo;

public class PrestamoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public int EjemplarId { get; set; }
    public string? CodigoEjemplar { get; set; }
    public int RecursoId { get; set; }
    public string? TituloRecurso { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string? MotivoRechazo { get; set; }

    // Un préstamo activo está vencido cuando su fecha límite ya pasó
    public bool Vencido { get; set; }

    // Días que faltan para la fecha límite. Es negativo cuando ya venció.
    public int? DiasRestantes { get; set; }
}