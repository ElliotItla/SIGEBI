using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;

namespace SIGEBI.Persistence.Interfaces.Seguridad;

public interface IUsuarioRepositorio : IRepositorioBase<Usuario>
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<Usuario?> ObtenerPorIdentificadorAsync(string identificador);
    Task<List<Usuario>> ObtenerPorFiltroAsync(string? tipo, bool? activo);
}