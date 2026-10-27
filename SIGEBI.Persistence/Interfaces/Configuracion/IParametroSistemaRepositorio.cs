using SIGEBI.DAL.Models.Configuracion;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Configuracion;

public interface IParametroSistemaRepositorio : IRepositorioBase<ParametroSistema>
{
    Task<ParametroSistema?> ObtenerPorClaveAsync(string clave);
}