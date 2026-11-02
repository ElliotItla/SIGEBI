using SIGEBI.BL.Dtos.Notificacion;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface INotificacionServicio
{
    Task<ResultadoOperacion> NotificarProximosVencimientosAsync();
    Task<ResultadoOperacion> ObtenerMisNotificacionesAsync(int usuarioId, bool soloNoLeidas);
    Task<ResultadoOperacion> ContarNoLeidasAsync(int usuarioId);
    Task<ResultadoOperacion> MarcarComoLeidasAsync(int usuarioId);
    Task<ResultadoOperacion> ObtenerRegistroAsync(FiltroNotificacionDto filtro);
}