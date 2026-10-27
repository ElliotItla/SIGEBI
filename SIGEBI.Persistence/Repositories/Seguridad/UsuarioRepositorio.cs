using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.Persistence.Repositories.Seguridad;

public class UsuarioRepositorio : RepositorioBase<Usuario>, IUsuarioRepositorio
{
    public UsuarioRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Para el inicio de sesión
    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return await Entidad.FirstOrDefaultAsync(u => u.Correo == correo);
    }

    public async Task<Usuario?> ObtenerPorIdentificadorAsync(string identificador)
    {
        return await Entidad.FirstOrDefaultAsync(u => u.Identificador == identificador);
    }

    // Para la lista de usuarios: filtra por tipo y por estado (activo o inactivo)
    public async Task<List<Usuario>> ObtenerPorFiltroAsync(string? tipo, bool? activo)
    {
        var consulta = Entidad.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            consulta = consulta.Where(u => u.Tipo == tipo);
        }

        if (activo.HasValue)
        {
            consulta = consulta.Where(u => u.Estatus == activo.Value);
        }

        return await consulta.OrderBy(u => u.Apellido).ThenBy(u => u.Nombre).ToListAsync();
    }
}