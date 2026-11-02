namespace SIGEBI.BL.Dtos.Reporte;

public class ReportePenalizacionesDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int Generadas { get; set; }
    public int Resueltas { get; set; }
    public int Activas { get; set; }
    public List<ConteoDto> PorTipoUsuario { get; set; } = new();
}