using System.Linq.Expressions;
using SIGEBI.DAL.Base;

namespace SIGEBI.DAL.Repository;

// Interfaz que heredan todos los repositorios.
// No hay método para borrar: las filas se dan de baja con Estatus (AuditEntity).
public interface IRepositorioBase<TEntidad> where TEntidad : class
{
    Task<TEntidad?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion> GuardarAsync(TEntidad entidad);
    Task<ResultadoOperacion> ActualizarAsync(TEntidad entidad);
    Task<List<TEntidad>> ObtenerTodosAsync();
    Task<ResultadoOperacion> ObtenerTodosAsync(Expression<Func<TEntidad, bool>> filtro);
    Task<bool> ExisteAsync(Expression<Func<TEntidad, bool>> filtro);
}