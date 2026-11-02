using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Context;

namespace SIGEBI.Persistence.Base;

public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly SigebiContext _contexto;

    public UnidadDeTrabajo(SigebiContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResultadoOperacion> EjecutarAsync(Func<Task<ResultadoOperacion>> operacion)
    {
        await using var transaccion = await _contexto.Database.BeginTransactionAsync();

        try
        {
            var resultado = await operacion();

            if (resultado.Exito)
            {
                await transaccion.CommitAsync();
            }
            else
            {
                await transaccion.RollbackAsync();
            }

            return resultado;
        }
        catch (Exception)
        {
            await transaccion.RollbackAsync();

            return new ResultadoOperacion
            {
                Exito = false,
                Mensaje = "Ocurrió un error y no se guardó ningún cambio."
            };
        }
    }
}