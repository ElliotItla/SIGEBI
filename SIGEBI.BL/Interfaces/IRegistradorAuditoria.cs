using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IRegistradorAuditoria
{
    // Guarda en el registro de auditoría quién hizo qué y con qué resultado (RN-AUD)
    Task<ResultadoOperacion> RegistrarAsync(int? usuarioId, string modulo, string tipoAccion, string resultado, string? detalle = null);
}