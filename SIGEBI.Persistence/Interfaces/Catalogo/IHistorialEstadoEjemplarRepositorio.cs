using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Catalogo;

public interface IHistorialEstadoEjemplarRepositorio : IRepositorioBase<HistorialEstadoEjemplar>
{
    Task<List<HistorialEstadoEjemplar>> ObtenerPorEjemplarAsync(int ejemplarId);
    Task<List<HistorialEstadoEjemplar>> ObtenerPorRecursoAsync(int recursoId);
}