using SIGEBI.BL.Dtos.Penalizacion;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IPenalizacionServicio
{
    Task<ResultadoOperacion> ResolverAsync(ResolverPenalizacionDto dto);
    Task<ResultadoOperacion> ObtenerActivasAsync(int usuarioAccionId);
    Task<ResultadoOperacion> ObtenerPorUsuarioAsync(int usuarioAccionId, int usuarioId, bool soloActivas);
}