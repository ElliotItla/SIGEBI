using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Context;

namespace SIGEBI.Persistence.Base;

// Repositorio genérico: guarda, actualiza y consulta.
// Cada repositorio de un módulo hereda de esta clase y agrega solo sus consultas propias.
public abstract class RepositorioBase<TEntidad> : IRepositorioBase<TEntidad> where TEntidad : class
{
    protected readonly SigebiContext _contexto;
    protected DbSet<TEntidad> Entidad { get; }

    protected RepositorioBase(SigebiContext contexto)
    {
        _contexto = contexto;
        Entidad = _contexto.Set<TEntidad>();
    }

    public virtual async Task<ResultadoOperacion> GuardarAsync(TEntidad entidad)
    {
        var resultado = new ResultadoOperacion();

        try
        {
            Entidad.Add(entidad);
            await _contexto.SaveChangesAsync();
        }
        catch (Exception)
        {
            resultado.Exito = false;
            resultado.Mensaje = "Ocurrió un error guardando los datos.";
        }

        return resultado;
    }

    public virtual async Task<ResultadoOperacion> ActualizarAsync(TEntidad entidad)
    {
        var resultado = new ResultadoOperacion();

        try
        {
            Entidad.Update(entidad);
            await _contexto.SaveChangesAsync();
        }
        catch (Exception)
        {
            resultado.Exito = false;
            resultado.Mensaje = "Ocurrió un error actualizando los datos.";
        }

        return resultado;
    }

    public virtual async Task<ResultadoOperacion> ObtenerTodosAsync(Expression<Func<TEntidad, bool>> filtro)
    {
        var resultado = new ResultadoOperacion();

        try
        {
            resultado.Datos = await Entidad.Where(filtro).ToListAsync();
        }
        catch (Exception)
        {
            resultado.Exito = false;
            resultado.Mensaje = "Ocurrió un error obteniendo los datos.";
        }

        return resultado;
    }

    public virtual async Task<TEntidad?> ObtenerPorIdAsync(int id)
    {
        return await Entidad.FindAsync(id);
    }

    public virtual async Task<bool> ExisteAsync(Expression<Func<TEntidad, bool>> filtro)
    {
        return await Entidad.AnyAsync(filtro);
    }

    public virtual async Task<List<TEntidad>> ObtenerTodosAsync()
    {
        return await Entidad.ToListAsync();
    }
}