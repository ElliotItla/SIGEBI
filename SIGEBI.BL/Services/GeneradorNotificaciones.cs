using SIGEBI.BL.Base;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.Persistence.Interfaces.Notificaciones;

namespace SIGEBI.BL.Services;

public class GeneradorNotificaciones : IGeneradorNotificaciones
{
    private readonly INotificacionRepositorio _notificaciones;

    public GeneradorNotificaciones(INotificacionRepositorio notificaciones)
    {
        _notificaciones = notificaciones;
    }

    public async Task<ResultadoOperacion> CrearAsync(int usuarioId, int? prestamoId, string tipoEvento, string mensaje)
    {
        var notificacion = new Notificacion
        {
            UsuarioId = usuarioId,
            PrestamoId = prestamoId,
            TipoEvento = tipoEvento,
            Mensaje = Texto.Recortar(mensaje, 300)
        };

        return await _notificaciones.GuardarAsync(notificacion);
    }
}