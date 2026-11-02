using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Devolucion;

public class RegistrarDevolucionDto : DtoBase
{
    // Código del ejemplar que se devuelve
    public string CodigoEjemplar { get; set; } = string.Empty;

    // Si el ejemplar llega con daños, queda fuera de servicio hasta que se revise
    public bool Danado { get; set; }

    public string? Observacion { get; set; }
}