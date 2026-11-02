using SIGEBI.BL.Dtos.Reporte;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IReporteServicio
{
    Task<ResultadoOperacion> ObtenerPrestamosAsync(FiltroReporteDto filtro);
    Task<ResultadoOperacion> ObtenerPenalizacionesAsync(FiltroReporteDto filtro);
    Task<ResultadoOperacion> ObtenerUsoCatalogoAsync(FiltroReporteDto filtro);
    Task<ResultadoOperacion> ObtenerInventarioAsync(int usuarioAccionId);
}