namespace SIGEBI.BL.Dtos.Penalizacion;

public class PenalizacionDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public int PrestamoId { get; set; }
    public string? TituloRecurso { get; set; }
    public int DiasRetraso { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? MotivoResolucion { get; set; }
}