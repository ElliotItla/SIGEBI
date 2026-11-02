namespace SIGEBI.BL.Dtos.Recurso;

public class HistorialEjemplarDto
{
    public DateTime Fecha { get; set; }
    public string? CodigoEjemplar { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public string? Responsable { get; set; }
    public string? Motivo { get; set; }
}