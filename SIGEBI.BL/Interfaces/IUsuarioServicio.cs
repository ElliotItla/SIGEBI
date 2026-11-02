using SIGEBI.BL.Base;
using SIGEBI.BL.Dtos.Usuario;
using SIGEBI.DAL.Base;

namespace SIGEBI.BL.Interfaces;

public interface IUsuarioServicio : IServicioBase<UsuarioDto>
{
    Task<ResultadoOperacion> IniciarSesionAsync(IniciarSesionDto dto);
    Task<ResultadoOperacion> GuardarAsync(GuardarUsuarioDto dto);
    Task<ResultadoOperacion> ActualizarAsync(ActualizarUsuarioDto dto);
    Task<ResultadoOperacion> CambiarEstadoAsync(CambiarEstadoUsuarioDto dto);
    Task<ResultadoOperacion> ObtenerPorFiltroAsync(FiltroUsuarioDto dto);
    Task<ResultadoOperacion> VerificarHabilitacionAsync(int usuarioId);
    Task<ResultadoOperacion> CrearAdministradorInicialAsync(string identificador, string nombre, string apellido, string correo, string contrasena);
}