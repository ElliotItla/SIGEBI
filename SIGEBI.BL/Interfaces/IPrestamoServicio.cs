using SIGEBI.BL.Dtos.Prestamo;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IPrestamoServicio
{
    Task<ResultadoOperacion> SolicitarAsync(SolicitarPrestamoDto dto);
    Task<ResultadoOperacion> AprobarAsync(AprobarPrestamoDto dto);
    Task<ResultadoOperacion> RechazarAsync(RechazarPrestamoDto dto);
    Task<ResultadoOperacion> ObtenerPendientesAsync(int usuarioAccionId);
    Task<ResultadoOperacion> ObtenerPorUsuarioAsync(int usuarioAccionId, int usuarioId, string? estado);
    Task<ResultadoOperacion> ObtenerPorRecursoAsync(int usuarioAccionId, int recursoId);
}