using SIGEBI.DAL.Models.Configuracion;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Configuracion;

public interface IPoliticaPrestamoRepositorio : IRepositorioBase<PoliticaPrestamo>
{
    Task<PoliticaPrestamo?> ObtenerPorTipoUsuarioAsync(string tipoUsuario);
}