using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Reporte;

// Período del reporte. Si no se indica, son los últimos 30 días.
public class FiltroReporteDto : DtoBase
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}