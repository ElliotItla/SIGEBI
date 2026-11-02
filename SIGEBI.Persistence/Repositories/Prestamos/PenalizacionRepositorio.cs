using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Prestamos;

namespace SIGEBI.Persistence.Repositories.Prestamos;

public class PenalizacionRepositorio : RepositorioBase<Penalizacion>, IPenalizacionRepositorio
{
    public PenalizacionRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Mientras un usuario tenga una penalización activa no puede solicitar préstamos
    public async Task<bool> TieneActivaAsync(int usuarioId)
    {
        return await Entidad.AnyAsync(p => p.UsuarioId == usuarioId && p.Estado == "Activa");
    }

    // Penalizaciones activas de todos los usuarios, para que el personal las resuelva
    public async Task<List<Penalizacion>> ObtenerActivasAsync()
    {
        return await Entidad
            .Include(p => p.Usuario)
            .Include(p => p.Prestamo.Ejemplar.Recurso)
            .Where(p => p.Estado == "Activa")
            .OrderBy(p => p.FechaCreacion)
            .ToListAsync();
    }

    // Penalizaciones de un usuario: solo las activas, o también las ya resueltas (historial)
    public async Task<List<Penalizacion>> ObtenerPorUsuarioAsync(int usuarioId, bool soloActivas)
    {
        var consulta = Entidad
            .Include(p => p.Prestamo.Ejemplar.Recurso)
            .Where(p => p.UsuarioId == usuarioId);

        if (soloActivas)
        {
            consulta = consulta.Where(p => p.Estado == "Activa");
        }

        return await consulta.OrderByDescending(p => p.FechaCreacion).ToListAsync();
    }
}