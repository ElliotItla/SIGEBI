namespace SIGEBI.Web.Models.Reportes;

// Período de un reporte. Sin fechas, el sistema usa los últimos 30 días.
public class PeriodoViewModel
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}