using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Base;

// Consultas que comparten los servicios de los módulos
public interface IServicioBase<TDto> where TDto : class
{
    Task<ResultadoOperacion> ObtenerTodosAsync();
    Task<ResultadoOperacion> ObtenerPorIdAsync(int id);
}