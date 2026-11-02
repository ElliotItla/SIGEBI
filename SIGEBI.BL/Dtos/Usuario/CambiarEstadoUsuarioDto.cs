using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Usuario;

// Activar o desactivar una cuenta. Desactivar no borra el historial del usuario.
public class CambiarEstadoUsuarioDto : DtoBase
{
    public int Id { get; set; }
    public bool Activo { get; set; }
}