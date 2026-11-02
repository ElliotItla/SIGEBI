namespace SIGEBI.BL.Dtos.Reporte;

public class RecursoSolicitadoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Solicitudes { get; set; }
}

public class ReporteUsoCatalogoDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public List<RecursoSolicitadoDto> MasSolicitados { get; set; } = new();
    public List<ConteoDto> DemandaPorCategoria { get; set; } = new();

    // Porcentaje de ejemplares disponibles en este momento
    public double? DisponibilidadActual { get; set; }
}