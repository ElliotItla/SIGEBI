using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Notificaciones;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Notificaciones;

namespace SIGEBI.Persistence.Repositories.Notificaciones;

public class NotificacionRepositorio : RepositorioBase<Notificacion>, INotificacionRepositorio
{
    public NotificacionRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Bandeja "Mis notificaciones": las más recientes primero
    public async Task<List<Notificacion>> ObtenerPorUsuarioAsync(int usuarioId, bool soloNoLeidas)
    {
        var consulta = Entidad.Where(n => n.UsuarioId == usuarioId);

        if (soloNoLeidas)
        {
            consulta = consulta.Where(n => !n.Leida);
        }

        return await consulta.OrderByDescending(n => n.FechaEnvio).ToListAsync();
    }

    // Para el aviso con la cantidad de notificaciones sin leer
    public async Task<int> ContarNoLeidasAsync(int usuarioId)
    {
        return await Entidad.CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
    }

    public async Task<ResultadoOperacion> MarcarComoLeidasAsync(int usuarioId)
    {
        var resultado = new ResultadoOperacion();

        try
        {
            var pendientes = await Entidad.Where(n => n.UsuarioId == usuarioId && !n.Leida).ToListAsync();

            foreach (var notificacion in pendientes)
            {
                notificacion.Leida = true;
            }

            await _contexto.SaveChangesAsync();
        }
        catch (Exception)
        {
            resultado.Exito = false;
            resultado.Mensaje = "Ocurrió un error actualizando los datos.";
        }

        return resultado;
    }

    // Registro de notificaciones enviadas, para el administrador y el auditor.
    // Cada filtro es opcional. "hasta" incluye el día completo.
    public async Task<List<Notificacion>> ObtenerRegistroAsync(int? usuarioId, string? tipoEvento, DateTime? desde, DateTime? hasta)
    {
        IQueryable<Notificacion> consulta = Entidad.Include(n => n.Usuario);

        if (usuarioId.HasValue)
        {
            consulta = consulta.Where(n => n.UsuarioId == usuarioId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tipoEvento))
        {
            consulta = consulta.Where(n => n.TipoEvento == tipoEvento);
        }

        if (desde.HasValue)
        {
            var inicio = desde.Value.Date;
            consulta = consulta.Where(n => n.FechaEnvio >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(n => n.FechaEnvio < fin);
        }

        return await consulta.OrderByDescending(n => n.FechaEnvio).ToListAsync();
    }
}