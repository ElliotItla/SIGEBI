using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Catalogo;

namespace SIGEBI.Persistence.Repositories.Catalogo;

public class EjemplarRepositorio : RepositorioBase<Ejemplar>, IEjemplarRepositorio
{
    public EjemplarRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    public async Task<Ejemplar?> ObtenerPorCodigoAsync(string codigoEjemplar)
    {
        return await Entidad.FirstOrDefaultAsync(e => e.CodigoEjemplar == codigoEjemplar);
    }

    // Elige un ejemplar disponible que no tenga ya otra solicitud pendiente
    public async Task<Ejemplar?> ObtenerDisponibleDeRecursoAsync(int recursoId)
    {
        return await Entidad
            .Where(e => e.RecursoId == recursoId && e.Estatus && e.Estado == "Disponible")
            .Where(e => !e.Prestamos.Any(p => p.Estado == "Pendiente"))
            .OrderBy(e => e.Id)
            .FirstOrDefaultAsync();
    }
}