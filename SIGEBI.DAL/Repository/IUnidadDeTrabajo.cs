using SIGEBI.DAL.Base;

namespace SIGEBI.DAL.Repository;

public interface IUnidadDeTrabajo
{
    // Ejecuta varias operaciones juntas: si alguna falla, no se guarda ninguna (RNF-TRZ-04)
    Task<ResultadoOperacion> EjecutarAsync(Func<Task<ResultadoOperacion>> operacion);
}