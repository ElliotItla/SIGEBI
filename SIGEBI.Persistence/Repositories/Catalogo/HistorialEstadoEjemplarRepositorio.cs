using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Catalogo;

namespace SIGEBI.Persistence.Repositories.Catalogo;

public class HistorialEstadoEjemplarRepositorio : RepositorioBase<HistorialEstadoEjemplar>, IHistorialEstadoEjemplarRepositorio
{
    public HistorialEstadoEjemplarRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Historial de un ejemplar, del cambio más reciente al más antiguo
    public async Task<List<HistorialEstadoEjemplar>> ObtenerPorEjemplarAsync(int ejemplarId)
    {
        return await Entidad
            .Include(h => h.Usuario)
            .Where(h => h.EjemplarId == ejemplarId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();
    }

    // Historial de todos los ejemplares de un recurso
    public async Task<List<HistorialEstadoEjemplar>> ObtenerPorRecursoAsync(int recursoId)
    {
        return await Entidad
            .Include(h => h.Ejemplar)
            .Include(h => h.Usuario)
            .Where(h => h.Ejemplar.RecursoId == recursoId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();
    }
}