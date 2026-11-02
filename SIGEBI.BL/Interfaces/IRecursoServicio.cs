using SIGEBI.BL.Dtos.Recurso;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IRecursoServicio
{
    Task<ResultadoOperacion> GuardarAsync(GuardarRecursoDto dto);
    Task<ResultadoOperacion> ActualizarAsync(ActualizarRecursoDto dto);
    Task<ResultadoOperacion> DarDeBajaAsync(BajaRecursoDto dto);
    Task<ResultadoOperacion> CambiarEstadoEjemplarAsync(CambiarEstadoEjemplarDto dto);
    Task<ResultadoOperacion> BuscarAsync(FiltroRecursoDto filtro);
    Task<ResultadoOperacion> ObtenerPorIdAsync(int usuarioAccionId, int recursoId);
    Task<ResultadoOperacion> ObtenerHistorialAsync(int usuarioAccionId, int recursoId);
}