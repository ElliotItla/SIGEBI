namespace SIGEBI.Web.Models.Reportes;

// Un reporte con su período. Datos queda vacío si el sistema no pudo generarlo.
public class ReporteViewModel<T> : PeriodoViewModel where T : class
{
    public T? Datos { get; set; }
}