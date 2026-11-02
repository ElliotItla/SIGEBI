using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Penalizacion;

public class ResolverPenalizacionDto : DtoBase
{
    public int PenalizacionId { get; set; }

    // Pago, cumplimiento de una condición o exoneración autorizada
    public string Motivo { get; set; } = string.Empty;
}