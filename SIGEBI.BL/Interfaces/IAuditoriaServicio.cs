using SIGEBI.BL.Dtos.Auditoria;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IAuditoriaServicio
{
    Task<ResultadoOperacion> ConsultarAsync(FiltroAuditoriaDto filtro);

    // Devuelve el resultado como texto CSV en Datos
    Task<ResultadoOperacion> ExportarCsvAsync(FiltroAuditoriaDto filtro);
}