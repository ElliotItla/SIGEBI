using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Auditoria;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Auditoria;

namespace SIGEBI.Persistence.Repositories.Auditoria;

public class RegistroAuditoriaRepositorio : RepositorioBase<RegistroAuditoria>, IRegistroAuditoriaRepositorio
{
    public RegistroAuditoriaRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // El registro de auditoría solo se agrega: nunca se modifica (la base de datos también lo impide)
    public override Task<ResultadoOperacion> ActualizarAsync(RegistroAuditoria entidad)
    {
        var resultado = new ResultadoOperacion
        {
            Exito = false,
            Mensaje = "El registro de auditoría no se puede modificar."
        };

        return Task.FromResult(resultado);
    }

    // Consulta del registro de auditoría. Cada filtro es opcional. "hasta" incluye el día completo.
    public async Task<List<RegistroAuditoria>> ConsultarAsync(int? usuarioId, string? modulo, string? tipoAccion, DateTime? desde, DateTime? hasta)
    {
        IQueryable<RegistroAuditoria> consulta = Entidad.Include(r => r.Usuario);

        if (usuarioId.HasValue)
        {
            consulta = consulta.Where(r => r.UsuarioId == usuarioId.Value);
        }

        if (!string.IsNullOrWhiteSpace(modulo))
        {
            consulta = consulta.Where(r => r.Modulo == modulo);
        }

        if (!string.IsNullOrWhiteSpace(tipoAccion))
        {
            consulta = consulta.Where(r => r.TipoAccion == tipoAccion);
        }

        if (desde.HasValue)
        {
            var inicio = desde.Value.Date;
            consulta = consulta.Where(r => r.Fecha >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(r => r.Fecha < fin);
        }

        return await consulta.OrderByDescending(r => r.Fecha).ToListAsync();
    }
}