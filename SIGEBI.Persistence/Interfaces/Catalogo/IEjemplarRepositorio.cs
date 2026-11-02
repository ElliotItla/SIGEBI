using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Catalogo;

public interface IEjemplarRepositorio : IRepositorioBase<Ejemplar>
{
    Task<Ejemplar?> ObtenerPorCodigoAsync(string codigoEjemplar);
    Task<Ejemplar?> ObtenerDisponibleDeRecursoAsync(int recursoId);
}