using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IGeneradorNotificaciones
{
    // Crea una notificación dentro del sistema para un usuario (RN-NOT)
    Task<ResultadoOperacion> CrearAsync(int usuarioId, int? prestamoId, string tipoEvento, string mensaje);
}