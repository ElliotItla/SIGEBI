using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Notificaciones;

public interface INotificacionRepositorio : IRepositorioBase<Notificacion>
{
    Task<List<Notificacion>> ObtenerPorUsuarioAsync(int usuarioId, bool soloNoLeidas);
    Task<int> ContarNoLeidasAsync(int usuarioId);
    Task<ResultadoOperacion> MarcarComoLeidasAsync(int usuarioId);
    Task<List<Notificacion>> ObtenerRegistroAsync(int? usuarioId, string? tipoEvento, DateTime? desde, DateTime? hasta);
}