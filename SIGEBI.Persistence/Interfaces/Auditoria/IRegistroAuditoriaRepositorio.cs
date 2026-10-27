using SIGEBI.DAL.Models.Auditoria;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Auditoria;

public interface IRegistroAuditoriaRepositorio : IRepositorioBase<RegistroAuditoria>
{
    Task<List<RegistroAuditoria>> ConsultarAsync(int? usuarioId, string? modulo, string? tipoAccion, DateTime? desde, DateTime? hasta);
}