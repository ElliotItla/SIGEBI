using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Catalogo;

namespace SIGEBI.Persistence.Repositories.Catalogo;

public class RecursoRepositorio : RepositorioBase<Recurso>, IRecursoRepositorio
{
    public RecursoRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Consulta del catálogo: cada filtro es opcional y solo se muestran los recursos que no están dados de baja
    public async Task<List<Recurso>> BuscarAsync(string? titulo, string? autor, string? categoria, bool soloDisponibles)
    {
        var consulta = Entidad.Include(r => r.Ejemplares).Where(r => r.Estatus);

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            consulta = consulta.Where(r => r.Titulo.Contains(titulo));
        }

        if (!string.IsNullOrWhiteSpace(autor))
        {
            consulta = consulta.Where(r => r.Autor.Contains(autor));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            consulta = consulta.Where(r => r.Categoria == categoria);
        }

        if (soloDisponibles)
        {
            consulta = consulta.Where(r => r.Ejemplares.Any(e => e.Estatus && e.Estado == "Disponible"));
        }

        return await consulta.OrderBy(r => r.Titulo).ToListAsync();
    }

    public async Task<Recurso?> ObtenerConEjemplaresAsync(int id)
    {
        return await Entidad.Include(r => r.Ejemplares).FirstOrDefaultAsync(r => r.Id == id);
    }

    // Un recurso con préstamos activos no se puede dar de baja
    public async Task<bool> TienePrestamosActivosAsync(int recursoId)
    {
        return await _contexto.Set<Prestamo>()
            .AnyAsync(p => p.Ejemplar.RecursoId == recursoId && p.Estado == "Activo");
    }
}