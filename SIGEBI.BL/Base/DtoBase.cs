namespace SIGEBI.BL.Base;

public abstract class DtoBase
{
    // Quién realiza la acción. Queda vacío cuando la hace el sistema (por ejemplo, los avisos automáticos)
    public int? UsuarioAccionId { get; set; }
}