using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Auditoria;

// Filtros de la consulta del registro de auditoría. Todos son opcionales.
public class FiltroAuditoriaDto : DtoBase
{
    public int? UsuarioId { get; set; }
    public string? Modulo { get; set; }
    public string? TipoAccion { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}