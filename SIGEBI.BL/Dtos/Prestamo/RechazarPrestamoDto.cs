using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Prestamo;

public class RechazarPrestamoDto : DtoBase
{
    public int PrestamoId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}