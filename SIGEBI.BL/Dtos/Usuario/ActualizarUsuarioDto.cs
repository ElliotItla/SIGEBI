using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Usuario;

public class ActualizarUsuarioDto : DtoBase
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;

    // Si viene con texto, el administrador asigna una contraseña nueva
    public string? NuevaContrasena { get; set; }
}