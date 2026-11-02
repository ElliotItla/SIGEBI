using SIGEBI.BL.Base;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Auditoria;
using SIGEBI.Persistence.Interfaces.Auditoria;

namespace SIGEBI.BL.Services;

public class RegistradorAuditoria : IRegistradorAuditoria
{
    private readonly IRegistroAuditoriaRepositorio _registros;

    public RegistradorAuditoria(IRegistroAuditoriaRepositorio registros)
    {
        _registros = registros;
    }

    public async Task<ResultadoOperacion> RegistrarAsync(int? usuarioId, string modulo, string tipoAccion, string resultado, string? detalle = null)
    {
        var registro = new RegistroAuditoria
        {
            UsuarioId = usuarioId,
            Modulo = Texto.Recortar(modulo, 30),
            TipoAccion = Texto.Recortar(tipoAccion, 60),
            Detalle = detalle == null ? null : Texto.Recortar(detalle, 300),
            Resultado = resultado
        };

        return await _registros.GuardarAsync(registro);
    }
}