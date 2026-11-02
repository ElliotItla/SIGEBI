using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Prestamos;

namespace SIGEBI.Persistence.Repositories.Prestamos;

public class PrestamoRepositorio : RepositorioBase<Prestamo>, IPrestamoRepositorio
{
    public PrestamoRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Para validar el límite de préstamos de un usuario
    public async Task<int> ContarActivosPorUsuarioAsync(int usuarioId)
    {
        return await Entidad.CountAsync(p => p.UsuarioId == usuarioId && p.Estado == "Activo");
    }

    // Un préstamo está vencido cuando sigue activo y su fecha límite ya pasó
    public async Task<bool> TieneVencidosAsync(int usuarioId)
    {
        return await Entidad.AnyAsync(p => p.UsuarioId == usuarioId && p.Estado == "Activo" && p.FechaLimite < DateTime.Now);
    }

    // Para registrar una devolución: el préstamo activo de un ejemplar
    public async Task<Prestamo?> ObtenerActivoPorEjemplarAsync(int ejemplarId)
    {
        return await Entidad
            .Include(p => p.Usuario)
            .Include(p => p.Ejemplar.Recurso)
            .FirstOrDefaultAsync(p => p.EjemplarId == ejemplarId && p.Estado == "Activo");
    }

    // Solicitudes que el personal bibliotecario debe aprobar o rechazar, la más antigua primero
    public async Task<List<Prestamo>> ObtenerPendientesAsync()
    {
        return await Entidad
            .Include(p => p.Usuario)
            .Include(p => p.Ejemplar.Recurso)
            .Where(p => p.Estado == "Pendiente")
            .OrderBy(p => p.FechaCreacion)
            .ToListAsync();
    }

    // Préstamos de un usuario. Sin estado trae todo el historial, con estado solo los de ese estado
    public async Task<List<Prestamo>> ObtenerPorUsuarioAsync(int usuarioId, string? estado)
    {
        var consulta = Entidad
            .Include(p => p.Ejemplar.Recurso)
            .Where(p => p.UsuarioId == usuarioId);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            consulta = consulta.Where(p => p.Estado == estado);
        }

        return await consulta.OrderByDescending(p => p.FechaCreacion).ToListAsync();
    }

    // Historial de préstamos de un recurso
    public async Task<List<Prestamo>> ObtenerPorRecursoAsync(int recursoId)
    {
        return await Entidad
            .Include(p => p.Usuario)
            .Include(p => p.Ejemplar)
            .Where(p => p.Ejemplar.RecursoId == recursoId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Prestamo>> ObtenerVencidosAsync()
    {
        return await Entidad
            .Include(p => p.Usuario)
            .Include(p => p.Ejemplar.Recurso)
            .Where(p => p.Estado == "Activo" && p.FechaLimite < DateTime.Now)
            .OrderBy(p => p.FechaLimite)
            .ToListAsync();
    }

    // Para el aviso de vencimiento: préstamos activos que vencen en los próximos días y todavía no se avisaron
    public async Task<List<Prestamo>> ObtenerProximosAVencerAsync(int diasAviso)
    {
        var ahora = DateTime.Now;
        var limite = ahora.AddDays(diasAviso);

        return await Entidad
            .Include(p => p.Ejemplar.Recurso)
            .Where(p => p.Estado == "Activo" && p.FechaLimite >= ahora && p.FechaLimite <= limite)
            .Where(p => !p.Notificaciones.Any(n => n.TipoEvento == "ProximoVencimiento"))
            .OrderBy(p => p.FechaLimite)
            .ToListAsync();
    }
}