namespace SIGEBI.BL.Dtos.Reporte;

public class ReportePrestamosDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public int TotalSolicitudes { get; set; }
    public int Pendientes { get; set; }
    public int Rechazados { get; set; }
    public int Activos { get; set; }
    public int Devueltos { get; set; }
    public int DevueltosATiempo { get; set; }
    public int DevueltosConRetraso { get; set; }

    // Porcentaje de devoluciones a tiempo. Vacío si todavía no hay devoluciones.
    public double? TasaDevolucionPuntual { get; set; }

    // Préstamos activos cuya fecha límite ya pasó
    public int VencidosActuales { get; set; }
}