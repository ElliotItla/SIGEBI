using SIGEBI.BL.Dtos.Usuario;

namespace SIGEBI.Web.Models.Usuarios;

public class UsuariosIndexViewModel
{
    public string? Tipo { get; set; }

    // "activos", "inactivos" o vacío para todos
    public string? Estado { get; set; }

    public List<UsuarioDto> Usuarios { get; set; } = new();

    // Solo el administrador crea, edita y activa o desactiva cuentas
    public bool PuedeAdministrar { get; set; }
}