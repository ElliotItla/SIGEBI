using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Recurso;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Catalogo;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Interfaces.Catalogo;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class RecursoServicio : IRecursoServicio
{
    private const int MaximoEjemplares = 100;
    private const int LargoMaximoMotivo = 200;

    private static readonly string[] EstadosPermitidos =
    {
        EstadosEjemplar.Disponible,
        EstadosEjemplar.Reservado,
        EstadosEjemplar.FueraDeServicio
    };

    private readonly IRecursoRepositorio _recursos;
    private readonly IEjemplarRepositorio _ejemplares;
    private readonly IHistorialEstadoEjemplarRepositorio _historial;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IUnidadDeTrabajo _unidad;

    public RecursoServicio(
        IRecursoRepositorio recursos,
        IEjemplarRepositorio ejemplares,
        IHistorialEstadoEjemplarRepositorio historial,
        IUsuarioRepositorio usuarios,
        IRegistradorAuditoria auditoria,
        IUnidadDeTrabajo unidad)
    {
        _recursos = recursos;
        _ejemplares = ejemplares;
        _historial = historial;
        _usuarios = usuarios;
        _auditoria = auditoria;
        _unidad = unidad;
    }

    // ---------- Registrar recurso con sus ejemplares (CU-CAT-01) ----------

    public async Task<ResultadoOperacion> GuardarAsync(GuardarRecursoDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "RegistrarRecurso", motivoActor);
        }

        var error = ValidarCodigo(dto.Codigo) ?? ValidarDatos(dto.Titulo, dto.Autor, dto.Categoria);

        if (error == null && (dto.CantidadEjemplares < 1 || dto.CantidadEjemplares > MaximoEjemplares))
        {
            error = $"La cantidad de ejemplares debe estar entre 1 y {MaximoEjemplares}.";
        }

        if (error != null)
        {
            return await RechazarAccionAsync(actor!.Id, "RegistrarRecurso", error);
        }

        var codigo = dto.Codigo.Trim();

        if (await _recursos.ExisteAsync(r => r.Codigo == codigo))
        {
            return await RechazarAccionAsync(actor!.Id, "RegistrarRecurso", "Ya existe un recurso con ese código.");
        }

        // Cada ejemplar lleva el código del recurso y un número: LIB-001-001, LIB-001-002...
        var codigos = Enumerable.Range(1, dto.CantidadEjemplares).Select(n => $"{codigo}-{n:000}").ToList();

        foreach (var codigoEjemplar in codigos)
        {
            if (await _ejemplares.ObtenerPorCodigoAsync(codigoEjemplar) != null)
            {
                return await RechazarAccionAsync(actor!.Id, "RegistrarRecurso", $"Ya existe un ejemplar con el código {codigoEjemplar}.");
            }
        }

        var recurso = new Recurso
        {
            Codigo = codigo,
            Titulo = dto.Titulo.Trim(),
            Autor = dto.Autor.Trim(),
            Categoria = dto.Categoria.Trim(),
            UsuarioCreacion = actor!.Id
        };

        var ahora = DateTime.Now;

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _recursos.GuardarAsync(recurso);

            if (!guardado.Exito)
            {
                return guardado;
            }

            var creados = new List<Ejemplar>();

            foreach (var codigoEjemplar in codigos)
            {
                var ejemplar = new Ejemplar
                {
                    RecursoId = recurso.Id,
                    CodigoEjemplar = codigoEjemplar,
                    Estado = EstadosEjemplar.Disponible,
                    UsuarioCreacion = actor.Id
                };

                guardado = await _ejemplares.GuardarAsync(ejemplar);

                if (!guardado.Exito)
                {
                    return guardado;
                }

                guardado = await _historial.GuardarAsync(new HistorialEstadoEjemplar
                {
                    EjemplarId = ejemplar.Id,
                    EstadoAnterior = null,
                    EstadoNuevo = EstadosEjemplar.Disponible,
                    Fecha = ahora,
                    UsuarioId = actor.Id,
                    Motivo = "Alta del ejemplar"
                });

                if (!guardado.Exito)
                {
                    return guardado;
                }

                creados.Add(ejemplar);
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Catalogo, "RegistrarRecurso",
                ResultadosAuditoria.Exitoso, $"Recurso {recurso.Codigo} con {creados.Count} ejemplar(es)");

            return auditado.Exito ? Correcto("El recurso se registró con sus ejemplares.", ADto(recurso, creados)) : auditado;
        });
    }

    // ---------- Actualizar, dar de baja y cambiar estado (CU-CAT-02, CU-CAT-03 y CU-CAT-04) ----------

    public async Task<ResultadoOperacion> ActualizarAsync(ActualizarRecursoDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "ActualizarRecurso", motivoActor);
        }

        var recurso = await _recursos.ObtenerPorIdAsync(dto.Id);

        if (recurso == null || !recurso.Estatus)
        {
            return await RechazarAccionAsync(actor!.Id, "ActualizarRecurso", "El recurso no existe.");
        }

        var error = ValidarDatos(dto.Titulo, dto.Autor, dto.Categoria);

        if (error != null)
        {
            return await RechazarAccionAsync(actor!.Id, "ActualizarRecurso", error);
        }

        recurso.Titulo = dto.Titulo.Trim();
        recurso.Autor = dto.Autor.Trim();
        recurso.Categoria = dto.Categoria.Trim();
        recurso.UsuarioModificacion = actor!.Id;

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _recursos.ActualizarAsync(recurso);

            if (!guardado.Exito)
            {
                return guardado;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Catalogo, "ActualizarRecurso",
                ResultadosAuditoria.Exitoso, $"Recurso {recurso.Codigo}");

            return auditado.Exito ? Correcto("El recurso se actualizó correctamente.", ADto(recurso, Array.Empty<Ejemplar>())) : auditado;
        });
    }

    // Solo el administrador da de baja un recurso, y nunca si tiene préstamos activos
    public async Task<ResultadoOperacion> DarDeBajaAsync(BajaRecursoDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "DarDeBajaRecurso", motivoActor);
        }

        var recurso = await _recursos.ObtenerConEjemplaresAsync(dto.Id);

        if (recurso == null)
        {
            return await RechazarAccionAsync(actor!.Id, "DarDeBajaRecurso", "El recurso no existe.");
        }

        if (!recurso.Estatus)
        {
            return await RechazarAccionAsync(actor!.Id, "DarDeBajaRecurso", "El recurso ya estaba dado de baja.");
        }

        if (await _recursos.TienePrestamosActivosAsync(recurso.Id))
        {
            return await RechazarAccionAsync(actor!.Id, "DarDeBajaRecurso", "No se puede dar de baja: el recurso tiene préstamos activos.");
        }

        recurso.Estatus = false;
        recurso.UsuarioModificacion = actor!.Id;

        foreach (var ejemplar in recurso.Ejemplares)
        {
            ejemplar.Estatus = false;
            ejemplar.UsuarioModificacion = actor.Id;
        }

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _recursos.ActualizarAsync(recurso);

            if (!guardado.Exito)
            {
                return guardado;
            }

            foreach (var ejemplar in recurso.Ejemplares)
            {
                guardado = await _ejemplares.ActualizarAsync(ejemplar);

                if (!guardado.Exito)
                {
                    return guardado;
                }
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Catalogo, "DarDeBajaRecurso",
                ResultadosAuditoria.Exitoso, $"Recurso {recurso.Codigo}");

            return auditado.Exito ? Correcto("El recurso se dio de baja. Su historial se conserva.") : auditado;
        });
    }

    public async Task<ResultadoOperacion> CambiarEstadoEjemplarAsync(CambiarEstadoEjemplarDto dto)
    {
        var (actor, motivoActor) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivoActor != null)
        {
            return await RechazarAccionAsync(actor?.Id, "CambiarEstadoEjemplar", motivoActor);
        }

        if (!EstadosPermitidos.Contains(dto.EstadoNuevo))
        {
            return await RechazarAccionAsync(actor!.Id, "CambiarEstadoEjemplar", "El estado debe ser Disponible, Reservado o FueraDeServicio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length > LargoMaximoMotivo)
        {
            return await RechazarAccionAsync(actor!.Id, "CambiarEstadoEjemplar", $"El motivo es obligatorio y no puede tener más de {LargoMaximoMotivo} caracteres.");
        }

        var ejemplar = await _ejemplares.ObtenerPorIdAsync(dto.EjemplarId);

        if (ejemplar == null || !ejemplar.Estatus)
        {
            return await RechazarAccionAsync(actor!.Id, "CambiarEstadoEjemplar", "El ejemplar no existe.");
        }

        if (ejemplar.Estado == EstadosEjemplar.Prestado)
        {
            return await RechazarAccionAsync(actor!.Id, "CambiarEstadoEjemplar", "El ejemplar está prestado: su estado cambia al registrar la devolución.");
        }

        if (ejemplar.Estado == dto.EstadoNuevo)
        {
            return await RechazarAccionAsync(actor!.Id, "CambiarEstadoEjemplar", $"El ejemplar ya está en estado {dto.EstadoNuevo}.");
        }

        var estadoAnterior = ejemplar.Estado;
        var motivo = dto.Motivo.Trim();

        ejemplar.Estado = dto.EstadoNuevo;
        ejemplar.UsuarioModificacion = actor!.Id;

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _ejemplares.ActualizarAsync(ejemplar);

            if (!guardado.Exito)
            {
                return guardado;
            }

            guardado = await _historial.GuardarAsync(new HistorialEstadoEjemplar
            {
                EjemplarId = ejemplar.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = dto.EstadoNuevo,
                Fecha = DateTime.Now,
                UsuarioId = actor.Id,
                Motivo = motivo
            });

            if (!guardado.Exito)
            {
                return guardado;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Catalogo, "CambiarEstadoEjemplar",
                ResultadosAuditoria.Exitoso, $"Ejemplar {ejemplar.CodigoEjemplar}: {estadoAnterior} a {dto.EstadoNuevo}");

            return auditado.Exito
                ? Correcto("El estado del ejemplar se actualizó.", new EjemplarDto { Id = ejemplar.Id, CodigoEjemplar = ejemplar.CodigoEjemplar, Estado = ejemplar.Estado })
                : auditado;
        });
    }

    // ---------- Consultas (CU-CAT-05 y CU-CAT-06) ----------

    // Cualquier usuario activo consulta el catálogo
    public async Task<ResultadoOperacion> BuscarAsync(FiltroRecursoDto filtro)
    {
        var (_, motivo) = await ObtenerActorAsync(filtro.UsuarioAccionId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var recursos = await _recursos.BuscarAsync(
            string.IsNullOrWhiteSpace(filtro.Titulo) ? null : filtro.Titulo.Trim(),
            string.IsNullOrWhiteSpace(filtro.Autor) ? null : filtro.Autor.Trim(),
            string.IsNullOrWhiteSpace(filtro.Categoria) ? null : filtro.Categoria.Trim(),
            filtro.SoloDisponibles);

        var lista = recursos.Select(r => ADto(r, r.Ejemplares)).ToList();

        return Correcto(lista.Count == 0 ? "No se encontraron coincidencias." : "Consulta realizada.", lista);
    }

    public async Task<ResultadoOperacion> ObtenerPorIdAsync(int usuarioAccionId, int recursoId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var recurso = await _recursos.ObtenerConEjemplaresAsync(recursoId);

        return recurso == null || !recurso.Estatus
            ? Rechazo("El recurso no existe.")
            : Correcto("Consulta realizada.", ADto(recurso, recurso.Ejemplares));
    }

    // Historial de cambios de estado de todos los ejemplares de un recurso
    public async Task<ResultadoOperacion> ObtenerHistorialAsync(int usuarioAccionId, int recursoId)
    {
        var (_, motivo) = await ObtenerActorAsync(usuarioAccionId,
            TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario, TiposUsuario.Auditor);

        if (motivo != null)
        {
            return Rechazo(motivo);
        }

        var recurso = await _recursos.ObtenerPorIdAsync(recursoId);

        if (recurso == null)
        {
            return Rechazo("El recurso no existe.");
        }

        var cambios = await _historial.ObtenerPorRecursoAsync(recursoId);

        var lista = cambios.Select(c => new HistorialEjemplarDto
        {
            Fecha = c.Fecha,
            CodigoEjemplar = c.Ejemplar?.CodigoEjemplar,
            EstadoAnterior = c.EstadoAnterior,
            EstadoNuevo = c.EstadoNuevo,
            Responsable = c.Usuario == null ? null : $"{c.Usuario.Nombre} {c.Usuario.Apellido}",
            Motivo = c.Motivo
        }).ToList();

        return Correcto("Consulta realizada.", lista);
    }

    // ---------- Apoyo ----------

    private static string? ValidarCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 30)
        {
            return "El código es obligatorio y no puede tener más de 30 caracteres.";
        }

        return null;
    }

    private static string? ValidarDatos(string? titulo, string? autor, string? categoria)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length > 200)
        {
            return "El título es obligatorio y no puede tener más de 200 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(autor) || autor.Trim().Length > 150)
        {
            return "El autor es obligatorio y no puede tener más de 150 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(categoria) || categoria.Trim().Length > 80)
        {
            return "La categoría es obligatoria y no puede tener más de 80 caracteres.";
        }

        return null;
    }

    private static ResultadoOperacion Rechazo(string mensaje)
    {
        return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }

    private static ResultadoOperacion Correcto(string mensaje, object? datos = null)
    {
        return new ResultadoOperacion { Exito = true, Mensaje = mensaje, Datos = datos };
    }

    private async Task<ResultadoOperacion> RechazarAccionAsync(int? usuarioId, string accion, string motivo)
    {
        await _auditoria.RegistrarAsync(usuarioId, Modulos.Catalogo, accion, ResultadosAuditoria.Rechazado, motivo);

        return Rechazo(motivo);
    }

    // Busca a quien realiza la acción. Si se indican tipos, solo esos tipos tienen permiso.
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

    // Solo cuenta los ejemplares que no están dados de baja
    private static RecursoDto ADto(Recurso recurso, IEnumerable<Ejemplar> ejemplares)
    {
        var activos = ejemplares.Where(e => e.Estatus).OrderBy(e => e.CodigoEjemplar).ToList();

        return new RecursoDto
        {
            Id = recurso.Id,
            Codigo = recurso.Codigo,
            Titulo = recurso.Titulo,
            Autor = recurso.Autor,
            Categoria = recurso.Categoria,
            TotalEjemplares = activos.Count,
            Disponibles = activos.Count(e => e.Estado == EstadosEjemplar.Disponible),
            Ejemplares = activos.Select(e => new EjemplarDto { Id = e.Id, CodigoEjemplar = e.CodigoEjemplar, Estado = e.Estado }).ToList()
        };
    }
}