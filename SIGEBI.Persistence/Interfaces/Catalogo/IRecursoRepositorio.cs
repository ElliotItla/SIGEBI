using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Catalogo;

public interface IRecursoRepositorio : IRepositorioBase<Recurso>
{
    Task<List<Recurso>> BuscarAsync(string? titulo, string? autor, string? categoria, bool soloDisponibles);
    Task<Recurso?> ObtenerConEjemplaresAsync(int id);
    Task<bool> TienePrestamosActivosAsync(int recursoId);
}