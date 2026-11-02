using System.Net.Mail;
using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Usuario;
using SIGEBI.BL.Interfaces;
using SIGEBI.DAL.Base;
using SIGEBI.DAL.Models.Seguridad;
using SIGEBI.DAL.Repository;
using SIGEBI.Persistence.Interfaces.Configuracion;
using SIGEBI.Persistence.Interfaces.Prestamos;
using SIGEBI.Persistence.Interfaces.Seguridad;

namespace SIGEBI.BL.Services;

public class UsuarioServicio : IUsuarioServicio
{
    private const string MensajeCredenciales = "Correo o contraseña incorrectos.";
    private const string ClaveMaxIntentos = "MaxIntentosLogin";
    private const string ClaveMinutosBloqueo = "MinutosBloqueo";
    private const int MaxIntentosPorDefecto = 5;
    private const int MinutosBloqueoPorDefecto = 15;
    private const int LargoMinimoContrasena = 8;

    private static readonly string[] TiposValidos =
    {
        TiposUsuario.Estudiante,
        TiposUsuario.Docente,
        TiposUsuario.PersonalBibliotecario,
        TiposUsuario.Administrador,
        TiposUsuario.Auditor
    };

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IParametroSistemaRepositorio _parametros;
    private readonly IPenalizacionRepositorio _penalizaciones;
    private readonly IPrestamoRepositorio _prestamos;
    private readonly IServicioContrasena _contrasenas;
    private readonly IRegistradorAuditoria _auditoria;
    private readonly IUnidadDeTrabajo _unidad;

    public UsuarioServicio(
        IUsuarioRepositorio usuarios,
        IParametroSistemaRepositorio parametros,
        IPenalizacionRepositorio penalizaciones,
        IPrestamoRepositorio prestamos,
        IServicioContrasena contrasenas,
        IRegistradorAuditoria auditoria,
        IUnidadDeTrabajo unidad)
    {
        _usuarios = usuarios;
        _parametros = parametros;
        _penalizaciones = penalizaciones;
        _prestamos = prestamos;
        _contrasenas = contrasenas;
        _auditoria = auditoria;
        _unidad = unidad;
    }

    // ---------- Consultas ----------

    public async Task<ResultadoOperacion> ObtenerTodosAsync()
    {
        var usuarios = await _usuarios.ObtenerPorFiltroAsync(null, null);

        return Correcto("Consulta realizada.", usuarios.Select(ADto).ToList());
    }

    public async Task<ResultadoOperacion> ObtenerPorIdAsync(int id)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(id);

        return usuario == null ? Rechazo("El usuario no existe.") : Correcto("Consulta realizada.", ADto(usuario));
    }

    // CU-USU-04: solo el administrador y el personal bibliotecario consultan la lista de usuarios
    public async Task<ResultadoOperacion> ObtenerPorFiltroAsync(FiltroUsuarioDto dto)
    {
        var (actor, motivo) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador, TiposUsuario.PersonalBibliotecario);

        if (motivo != null)
        {
            return await RechazarAsync(actor?.Id, "ConsultarUsuarios", motivo);
        }

        var usuarios = await _usuarios.ObtenerPorFiltroAsync(dto.Tipo, dto.Activo);

        return Correcto("Consulta realizada.", usuarios.Select(ADto).ToList());
    }

    // ---------- Inicio de sesión (CU-USU-01) ----------

    public async Task<ResultadoOperacion> IniciarSesionAsync(IniciarSesionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrEmpty(dto.Contrasena))
        {
            return Rechazo("Escribe tu correo y tu contraseña.");
        }

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObtenerPorCorreoAsync(correo);

        if (usuario == null)
        {
            return await RechazarAsync(null, "InicioSesion", "Correo no registrado", MensajeCredenciales);
        }

        if (!usuario.Estatus)
        {
            return await RechazarAsync(usuario.Id, "InicioSesion", "Cuenta desactivada", "La cuenta está desactivada. Comunícate con la biblioteca.");
        }

        var ahora = DateTime.Now;

        if (usuario.BloqueadoHasta != null && usuario.BloqueadoHasta > ahora)
        {
            return await RechazarAsync(usuario.Id, "InicioSesion", "Cuenta bloqueada",
                $"La cuenta está bloqueada temporalmente hasta las {usuario.BloqueadoHasta:HH:mm}. Intenta más tarde.");
        }

        if (!_contrasenas.Verificar(dto.Contrasena, usuario.ContrasenaHash))
        {
            return await RegistrarIntentoFallidoAsync(usuario, ahora);
        }

        // Entró bien: se borran los intentos fallidos y el bloqueo vencido
        if (usuario.IntentosFallidos != 0 || usuario.BloqueadoHasta != null)
        {
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
            usuario.UsuarioModificacion = usuario.Id;

            var guardado = await _usuarios.ActualizarAsync(usuario);

            if (!guardado.Exito)
            {
                return guardado;
            }
        }

        await _auditoria.RegistrarAsync(usuario.Id, Modulos.Usuarios, "InicioSesion", ResultadosAuditoria.Exitoso);

        return Correcto("Inicio de sesión correcto.", ADto(usuario));
    }

    private async Task<ResultadoOperacion> RegistrarIntentoFallidoAsync(Usuario usuario, DateTime ahora)
    {
        var maximo = await LeerParametroAsync(ClaveMaxIntentos, MaxIntentosPorDefecto);
        var minutos = await LeerParametroAsync(ClaveMinutosBloqueo, MinutosBloqueoPorDefecto);
        var mensaje = MensajeCredenciales;

        usuario.IntentosFallidos++;

        if (usuario.IntentosFallidos >= maximo)
        {
            usuario.BloqueadoHasta = ahora.AddMinutes(minutos);
            usuario.IntentosFallidos = 0;
            mensaje = $"Demasiados intentos fallidos. La cuenta se bloqueó por {minutos} minutos.";
        }

        usuario.UsuarioModificacion = usuario.Id;

        var guardado = await _usuarios.ActualizarAsync(usuario);

        if (!guardado.Exito)
        {
            return guardado;
        }

        return await RechazarAsync(usuario.Id, "InicioSesion", "Contraseña incorrecta", mensaje);
    }

    // ---------- Registro y administración (CU-USU-02 y CU-USU-03) ----------

    public async Task<ResultadoOperacion> GuardarAsync(GuardarUsuarioDto dto)
    {
        var (actor, motivo) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador);

        if (motivo != null)
        {
            return await RechazarAsync(actor?.Id, "RegistrarUsuario", motivo);
        }

        var error = ValidarIdentificador(dto.Identificador)
            ?? ValidarDatos(dto.Nombre, dto.Apellido, dto.Correo, dto.Tipo)
            ?? ValidarContrasena(dto.Contrasena);

        if (error != null)
        {
            return await RechazarAsync(actor!.Id, "RegistrarUsuario", error);
        }

        var identificador = dto.Identificador.Trim();
        var correo = dto.Correo.Trim().ToLowerInvariant();

        if (await _usuarios.ExisteAsync(u => u.Identificador == identificador))
        {
            return await RechazarAsync(actor!.Id, "RegistrarUsuario", "Ya existe un usuario con ese identificador.");
        }

        if (await _usuarios.ExisteAsync(u => u.Correo == correo))
        {
            return await RechazarAsync(actor!.Id, "RegistrarUsuario", "Ya existe un usuario con ese correo.");
        }

        var usuario = new Usuario
        {
            Identificador = identificador,
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Correo = correo,
            ContrasenaHash = _contrasenas.Cifrar(dto.Contrasena),
            Tipo = dto.Tipo,
            UsuarioCreacion = actor!.Id
        };

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _usuarios.GuardarAsync(usuario);

            if (!guardado.Exito)
            {
                return guardado;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Usuarios, "RegistrarUsuario",
                ResultadosAuditoria.Exitoso, $"Usuario {usuario.Identificador} ({usuario.Tipo})");

            return auditado.Exito ? Correcto("El usuario se registró correctamente.", ADto(usuario)) : auditado;
        });
    }

    public async Task<ResultadoOperacion> ActualizarAsync(ActualizarUsuarioDto dto)
    {
        var (actor, motivo) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador);

        if (motivo != null)
        {
            return await RechazarAsync(actor?.Id, "ActualizarUsuario", motivo);
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(dto.Id);

        if (usuario == null)
        {
            return await RechazarAsync(actor!.Id, "ActualizarUsuario", "El usuario no existe.");
        }

        var error = ValidarDatos(dto.Nombre, dto.Apellido, dto.Correo, dto.Tipo);

        if (error == null && !string.IsNullOrEmpty(dto.NuevaContrasena))
        {
            error = ValidarContrasena(dto.NuevaContrasena);
        }

        if (error == null && usuario.Id == actor!.Id && dto.Tipo != usuario.Tipo)
        {
            error = "No puedes cambiar tu propio tipo de usuario.";
        }

        if (error != null)
        {
            return await RechazarAsync(actor!.Id, "ActualizarUsuario", error);
        }

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var idUsuario = usuario.Id;

        if (await _usuarios.ExisteAsync(u => u.Correo == correo && u.Id != idUsuario))
        {
            return await RechazarAsync(actor!.Id, "ActualizarUsuario", "Ya existe otro usuario con ese correo.");
        }

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Apellido = dto.Apellido.Trim();
        usuario.Correo = correo;
        usuario.Tipo = dto.Tipo;
        usuario.UsuarioModificacion = actor!.Id;

        if (!string.IsNullOrEmpty(dto.NuevaContrasena))
        {
            usuario.ContrasenaHash = _contrasenas.Cifrar(dto.NuevaContrasena);
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = null;
        }

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _usuarios.ActualizarAsync(usuario);

            if (!guardado.Exito)
            {
                return guardado;
            }

            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Usuarios, "ActualizarUsuario",
                ResultadosAuditoria.Exitoso, $"Usuario {usuario.Identificador}");

            return auditado.Exito ? Correcto("El usuario se actualizó correctamente.", ADto(usuario)) : auditado;
        });
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(CambiarEstadoUsuarioDto dto)
    {
        var (actor, motivo) = await ObtenerActorAsync(dto.UsuarioAccionId, TiposUsuario.Administrador);

        if (motivo != null)
        {
            return await RechazarAsync(actor?.Id, "CambiarEstadoUsuario", motivo);
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(dto.Id);

        if (usuario == null)
        {
            return await RechazarAsync(actor!.Id, "CambiarEstadoUsuario", "El usuario no existe.");
        }

        if (usuario.Id == actor!.Id && !dto.Activo)
        {
            return await RechazarAsync(actor.Id, "CambiarEstadoUsuario", "No puedes desactivar tu propia cuenta.");
        }

        if (usuario.Estatus == dto.Activo)
        {
            return Correcto(dto.Activo ? "El usuario ya estaba activo." : "El usuario ya estaba inactivo.", ADto(usuario));
        }

        usuario.Estatus = dto.Activo;
        usuario.UsuarioModificacion = actor.Id;

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _usuarios.ActualizarAsync(usuario);

            if (!guardado.Exito)
            {
                return guardado;
            }

            var accion = dto.Activo ? "ActivarUsuario" : "DesactivarUsuario";
            var auditado = await _auditoria.RegistrarAsync(actor.Id, Modulos.Usuarios, accion,
                ResultadosAuditoria.Exitoso, $"Usuario {usuario.Identificador}");

            return auditado.Exito
                ? Correcto(dto.Activo ? "El usuario se activó." : "El usuario se desactivó.", ADto(usuario))
                : auditado;
        });
    }

    // Crea el primer administrador cuando todavía no hay ningún usuario
    public async Task<ResultadoOperacion> CrearAdministradorInicialAsync(string identificador, string nombre, string apellido, string correo, string contrasena)
    {
        if (await _usuarios.ExisteAsync(u => true))
        {
            return Correcto("Ya existen usuarios, no se creó el administrador inicial.");
        }

        var error = ValidarIdentificador(identificador)
            ?? ValidarDatos(nombre, apellido, correo, TiposUsuario.Administrador)
            ?? ValidarContrasena(contrasena);

        if (error != null)
        {
            return Rechazo(error);
        }

        var usuario = new Usuario
        {
            Identificador = identificador.Trim(),
            Nombre = nombre.Trim(),
            Apellido = apellido.Trim(),
            Correo = correo.Trim().ToLowerInvariant(),
            ContrasenaHash = _contrasenas.Cifrar(contrasena),
            Tipo = TiposUsuario.Administrador
        };

        return await _unidad.EjecutarAsync(async () =>
        {
            var guardado = await _usuarios.GuardarAsync(usuario);

            if (!guardado.Exito)
            {
                return guardado;
            }

            var auditado = await _auditoria.RegistrarAsync(null, Modulos.Usuarios, "CrearAdministradorInicial",
                ResultadosAuditoria.Exitoso, $"Usuario {usuario.Identificador}");

            return auditado.Exito ? Correcto("Se creó el administrador inicial.", ADto(usuario)) : auditado;
        });
    }

    // ---------- Habilitación (CU-USU-05) ----------

    // Antes de aceptar una solicitud de préstamo se comprueba que el usuario puede pedir libros
    public async Task<ResultadoOperacion> VerificarHabilitacionAsync(int usuarioId)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);

        if (usuario == null)
        {
            return Rechazo("El usuario no existe.");
        }

        if (!usuario.Estatus)
        {
            return Rechazo("El usuario está inactivo.");
        }

        if (usuario.Tipo != TiposUsuario.Estudiante && usuario.Tipo != TiposUsuario.Docente)
        {
            return Rechazo("Este tipo de usuario no solicita préstamos.");
        }

        if (await _penalizaciones.TieneActivaAsync(usuarioId))
        {
            return Rechazo("El usuario tiene una penalización activa.");
        }

        if (await _prestamos.TieneVencidosAsync(usuarioId))
        {
            return Rechazo("El usuario tiene un préstamo vencido sin devolver.");
        }

        return Correcto("El usuario está habilitado.");
    }

    // ---------- Apoyo ----------

    private static ResultadoOperacion Rechazo(string mensaje)
    {
        return new ResultadoOperacion { Exito = false, Mensaje = mensaje };
    }

    private static ResultadoOperacion Correcto(string mensaje, object? datos = null)
    {
        return new ResultadoOperacion { Exito = true, Mensaje = mensaje, Datos = datos };
    }

    // Registra el rechazo en la auditoría y devuelve el mensaje al usuario. Si no se da un mensaje, se usa el motivo.
    private async Task<ResultadoOperacion> RechazarAsync(int? usuarioId, string accion, string motivo, string? mensaje = null)
    {
        await _auditoria.RegistrarAsync(usuarioId, Modulos.Usuarios, accion, ResultadosAuditoria.Rechazado, motivo);

        return Rechazo(mensaje ?? motivo);
    }

    private static UsuarioDto ADto(Usuario usuario)
    {
        return new UsuarioDto
        {
            Id = usuario.Id,
            Identificador = usuario.Identificador,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Tipo = usuario.Tipo,
            Activo = usuario.Estatus,
            BloqueadoHasta = usuario.BloqueadoHasta
        };
    }

    // Busca a quien realiza la acción y comprueba que su tipo tenga permiso
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

        if (!tiposPermitidos.Contains(actor.Tipo))
        {
            return (actor, "No tienes permiso para realizar esta acción.");
        }

        return (actor, null);
    }

    private async Task<int> LeerParametroAsync(string clave, int valorPorDefecto)
    {
        var parametro = await _parametros.ObtenerPorClaveAsync(clave);

        return parametro != null && parametro.Valor > 0 ? parametro.Valor : valorPorDefecto;
    }

    private static string? ValidarIdentificador(string? identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador) || identificador.Trim().Length > 30)
        {
            return "El identificador es obligatorio y no puede tener más de 30 caracteres.";
        }

        return null;
    }

    private static string? ValidarDatos(string? nombre, string? apellido, string? correo, string? tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 80)
        {
            return "El nombre es obligatorio y no puede tener más de 80 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(apellido) || apellido.Trim().Length > 80)
        {
            return "El apellido es obligatorio y no puede tener más de 80 caracteres.";
        }

        if (!CorreoValido(correo))
        {
            return "El correo no es válido.";
        }

        if (tipo == null || !TiposValidos.Contains(tipo))
        {
            return "El tipo de usuario no es válido.";
        }

        return null;
    }

    private static string? ValidarContrasena(string? contrasena)
    {
        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < LargoMinimoContrasena)
        {
            return $"La contraseña debe tener al menos {LargoMinimoContrasena} caracteres.";
        }

        return null;
    }

    private static bool CorreoValido(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo) || correo.Trim().Length > 120)
        {
            return false;
        }

        var texto = correo.Trim();

        return MailAddress.TryCreate(texto, out var direccion) && direccion.Address.Equals(texto, StringComparison.OrdinalIgnoreCase);
    }
}