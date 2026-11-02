using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Reporte;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Prestamos;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class ReporteServicio : IReporteServicio
{
    private const int DiasPorDefecto = 30;
    private const int MaximoDiasPeriodo = 366;
    private const int MaximoMasSolicitados = 10;

    private readonly IPrestamoRepositorio _prestamos;
    private readonly IPenalizacionRepositorio _penalizaciones;
    private readonly IEjemplarRepositorio _ejemplares;
    private readonly IRecursoRepositorio _recursos;
    private readonly IUsuarioRepositorio _usuarios;

    public ReporteServicio(
        IPrestamoRepositorio prestamos,
        IPenalizacionRepositorio penalizaciones,
        IEjemplarRepositorio ejemplares,
        IRecursoRepositorio recursos,
        IUsuarioRepositorio usuarios)
    {
        _prestamos = prestamos;
        _penalizaciones = penalizaciones;
        _ejemplares = ejemplares;
        _recursos = recursos;
        _usuarios = usuarios;
    }

    // ---------- Reporte de préstamos (RF-REP-02) ----------

    public async Task<ResultadoOperacion> ObtenerPrestamosAsync(FiltroReporteDto filtro)
    {
        var (desde, hasta, motivo) = await PrepararAsync(filtro, TiposUsuario.Administrador, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var consulta = await _prestamos.ObtenerTodosAsync(p => p.FechaCreacion >= desde && p.FechaCreacion < hasta.AddDays(1));

        if (!consulta.Exito)
        {
            return consulta;
        }

        var prestamos = Lista<Prestamo>(consulta);
        var devueltos = prestamos.Where(p => p.Estado == EstadosPrestamo.Devuelto).ToList();
        var aTiempo = devueltos.Count(p => p.FechaLimite == null || p.FechaDevolucion == null || p.FechaDevolucion <= p.FechaLimite);
        var ahora = DateTime.Now;

        return Correcto("Reporte generado.", new ReportePrestamosDto
        {
            Desde = desde,
            Hasta = hasta,
            TotalSolicitudes = prestamos.Count,
            Pendientes = prestamos.Count(p => p.Estado == EstadosPrestamo.Pendiente),
            Rechazados = prestamos.Count(p => p.Estado == EstadosPrestamo.Rechazado),
            Activos = prestamos.Count(p => p.Estado == EstadosPrestamo.Activo),
            Devueltos = devueltos.Count,
            DevueltosATiempo = aTiempo,
            DevueltosConRetraso = devueltos.Count - aTiempo,
            TasaDevolucionPuntual = Porcentaje(aTiempo, devueltos.Count),
            VencidosActuales = prestamos.Count(p => p.Estado == EstadosPrestamo.Activo && p.FechaLimite < ahora)
        });
    }

    // ---------- Reporte de penalizaciones (RF-REP-03) ----------

    public async Task<ResultadoOperacion> ObtenerPenalizacionesAsync(FiltroReporteDto filtro)
    {
        var (desde, hasta, motivo) = await PrepararAsync(filtro, TiposUsuario.Administrador, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var consulta = await _penalizaciones.ObtenerTodosAsync(p => p.FechaCreacion >= desde && p.FechaCreacion < hasta.AddDays(1));

        if (!consulta.Exito)
        {
            return consulta;
        }

        var penalizaciones = Lista<Penalizacion>(consulta);
        var tipos = (await _usuarios.ObtenerTodosAsync()).ToDictionary(u => u.Id, u => u.Tipo);

        return Correcto("Reporte generado.", new ReportePenalizacionesDto
        {
            Desde = desde,
            Hasta = hasta,
            Generadas = penalizaciones.Count,
            Resueltas = penalizaciones.Count(p => p.Estado == EstadosPenalizacion.Resuelta),
            Activas = penalizaciones.Count(p => p.Estado == EstadosPenalizacion.Activa),
            PorTipoUsuario = penalizaciones
                .GroupBy(p => tipos.GetValueOrDefault(p.UsuarioId, "Desconocido"))
                .Select(g => new ConteoDto { Nombre = g.Key, Cantidad = g.Count() })
                .OrderBy(c => c.Nombre)
                .ToList()
        });
    }

    // ---------- Reporte de uso del catálogo (RF-REP-01) ----------

    public async Task<ResultadoOperacion> ObtenerUsoCatalogoAsync(FiltroReporteDto filtro)
    {
        var (desde, hasta, motivo) = await PrepararAsync(filtro, TiposUsuario.Administrador, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var consulta = await _prestamos.ObtenerTodosAsync(p => p.FechaCreacion >= desde && p.FechaCreacion < hasta.AddDays(1));

        if (!consulta.Exito)
        {
            return consulta;
        }

        var prestamos = Lista<Prestamo>(consulta);
        var ejemplares = await _ejemplares.ObtenerTodosAsync();
        var recursos = (await _recursos.ObtenerTodosAsync()).ToDictionary(r => r.Id);
        var recursoDeEjemplar = ejemplares.ToDictionary(e => e.Id, e => e.RecursoId);

        // Cada solicitud cuenta para el recurso al que pertenece su ejemplar
        var solicitudes = prestamos
            .Where(p => recursoDeEjemplar.ContainsKey(p.EjemplarId) && recursos.ContainsKey(recursoDeEjemplar[p.EjemplarId]))
            .Select(p => recursos[recursoDeEjemplar[p.EjemplarId]])
            .ToList();

        var activos = ejemplares.Where(e => e.Estatus).ToList();

        return Correcto("Reporte generado.", new ReporteUsoCatalogoDto
        {
            Desde = desde,
            Hasta = hasta,
            MasSolicitados = solicitudes
                .GroupBy(r => r)
                .Select(g => new RecursoSolicitadoDto { Codigo = g.Key.Codigo, Titulo = g.Key.Titulo, Categoria = g.Key.Categoria, Solicitudes = g.Count() })
                .OrderByDescending(r => r.Solicitudes)
                .ThenBy(r => r.Titulo)
                .Take(MaximoMasSolicitados)
                .ToList(),
            DemandaPorCategoria = solicitudes
                .GroupBy(r => r.Categoria)
                .Select(g => new ConteoDto { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(c => c.Cantidad)
                .ThenBy(c => c.Nombre)
                .ToList(),
            DisponibilidadActual = Porcentaje(activos.Count(e => e.Estado == EstadosEjemplar.Disponible), activos.Count)
        });
    }

    // ---------- Reporte de inventario (RF-REP-05) ----------

    public async Task<ResultadoOperacion> ObtenerInventarioAsync(int usuarioAccionId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId,
            TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        // Solo cuenta lo que no está dado de baja
        var recursos = (await _recursos.ObtenerTodosAsync()).Where(r => r.Estatus).ToList();
        var ejemplares = (await _ejemplares.ObtenerTodosAsync()).Where(e => e.Estatus).ToList();

        var porRecurso = recursos
            .Select(r =>
            {
                var suyos = ejemplares.Where(e => e.RecursoId == r.Id).ToList();

                return new InventarioRecursoDto
                {
                    Codigo = r.Codigo,
                    Titulo = r.Titulo,
                    Categoria = r.Categoria,
                    Ejemplares = suyos.Count,
                    Disponibles = suyos.Count(e => e.Estado == EstadosEjemplar.Disponible)
                };
            })
            .OrderBy(r => r.Titulo)
            .ToList();

        return Correcto("Reporte generado.", new ReporteInventarioDto
        {
            TotalRecursos = recursos.Count,
            TotalEjemplares = ejemplares.Count,
            Disponibles = ejemplares.Count(e => e.Estado == EstadosEjemplar.Disponible),
            Prestados = ejemplares.Count(e => e.Estado == EstadosEjemplar.Prestado),
            Reservados = ejemplares.Count(e => e.Estado == EstadosEjemplar.Reservado),
            FueraDeServicio = ejemplares.Count(e => e.Estado == EstadosEjemplar.FueraDeServicio),
            PorCategoria = porRecurso
                .GroupBy(r => r.Categoria)
                .Select(g => new InventarioCategoriaDto
                {
                    Categoria = g.Key,
                    Recursos = g.Count(),
                    Ejemplares = g.Sum(r => r.Ejemplares),
                    Disponibles = g.Sum(r => r.Disponibles)
                })
                .OrderBy(c => c.Categoria)
                .ToList(),
            PorRecurso = porRecurso
        });
    }

    // ---------- Apoyo ----------

    // Comprueba permisos y deja el período listo: sin fechas son los últimos 30 días
    private async Task<(DateTime Desde, DateTime Hasta, string? Motivo)> PrepararAsync(FiltroReporteDto filtro, params string[] tiposPermitidos)
    {
        var (_, motivo) = await ObtenerActorAsync(filtro.UsuarioAccionId, tiposPermitidos);

        if (motivo != null)
        {
            return (default, default, motivo);
        }

        var hasta = (filtro.Hasta ?? DateTime.Now).Date;
        var desde = (filtro.Desde ?? hasta.AddDays(-(DiasPorDefecto - 1))).Date;

        if (desde > hasta)
        {
            return (default, default, "La fecha inicial no puede ser posterior a la final.");
        }

        if ((hasta - desde).Days >= MaximoDiasPeriodo)
        {
            return (default, default, $"El período no puede ser mayor a {MaximoDiasPeriodo} días.");
        }

        return (desde, hasta, null);
    }

    // Porcentaje con un decimal. Vacío cuando no hay con qué calcularlo.
    private static double? Porcentaje(int parte, int total)
    {
        return total == 0 ? null : Math.Round(100.0 * parte / total, 1);
    }

    private static List<T> Lista<T>(ResultadoOperacion resultado)
    {
        return resultado.Datos as List<T> ?? new List<T>();
    }

    private static ResultadoOperacion Rechazo(string mensaje)
    {
        return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }

    private static ResultadoOperacion Correcto(string mensaje, object? datos = null)
    {
        return new ResultadoOperacion { Exito = true, Mensaje = mensaje, Datos = datos };
    }

    private async Task<(Usuario? Actor, string? Motivo)> ObtenerActorAsync(int? usuarioAccionId, params string[] tiposPermitidos)
    {
        if (usuarioAccionId == null)
        {
            return (null, "No se indicó quién realiza la acción.");
        }

        var actor = await _usuarios.ObtenerPorIdAsync(usuarioAccionId.Value);

        if (actor == null || !actor.Estatus)
        {
            return (null, "El usuario que realiza la acción no existe o está inactivo.");
        }

        if (tiposPermitidos.Length > 0 && !tiposPermitidos.Contains(actor.Tipo))
        {
            return (actor, "No tienes permiso para realizar esta acción.");
        }

        return (actor, null);
    }
}