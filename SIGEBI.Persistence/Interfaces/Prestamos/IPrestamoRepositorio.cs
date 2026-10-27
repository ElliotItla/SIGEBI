using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Prestamos;

public interface IPrestamoRepositorio : IRepositorioBase<Prestamo>
{
    Task<int> ContarActivosPorUsuarioAsync(int usuarioId);
    Task<bool> TieneVencidosAsync(int usuarioId);
    Task<Prestamo?> ObtenerActivoPorEjemplarAsync(int ejemplarId);
    Task<List<Prestamo>> ObtenerPendientesAsync();
    Task<List<Prestamo>> ObtenerPorUsuarioAsync(int usuarioId, string? estado);
    Task<List<Prestamo>> ObtenerPorRecursoAsync(int recursoId);
    Task<List<Prestamo>> ObtenerVencidosAsync();
    Task<List<Prestamo>> ObtenerProximosAVencerAsync(int diasAviso);
}