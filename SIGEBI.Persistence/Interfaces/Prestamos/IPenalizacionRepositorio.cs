using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Prestamos;

public interface IPenalizacionRepositorio : IRepositorioBase<Penalizacion>
{
    Task<bool> TieneActivaAsync(int usuarioId);
    Task<List<Penalizacion>> ObtenerActivasAsync();
    Task<List<Penalizacion>> ObtenerPorUsuarioAsync(int usuarioId, bool soloActivas);
}