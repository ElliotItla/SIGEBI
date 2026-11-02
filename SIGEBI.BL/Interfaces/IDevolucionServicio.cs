using SIGEBI.BL.Dtos.Devolucion;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IDevolucionServicio
{
    Task<ResultadoOperacion> RegistrarAsync(RegistrarDevolucionDto dto);
    Task<ResultadoOperacion> ObtenerHistorialPorUsuarioAsync(int usuarioAccionId, int usuarioId);
    Task<ResultadoOperacion> ObtenerHistorialPorRecursoAsync(int usuarioAccionId, int recursoId);
}