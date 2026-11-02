using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Usuario;

public class GuardarUsuarioDto : DtoBase
{
    public string Identificador { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}