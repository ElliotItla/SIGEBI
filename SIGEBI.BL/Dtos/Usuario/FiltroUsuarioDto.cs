using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Usuario;

public class FiltroUsuarioDto : DtoBase
{
    public string? Tipo { get; set; }
    public bool? Activo { get; set; }
}