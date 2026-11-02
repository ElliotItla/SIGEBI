using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Recurso;

public class CambiarEstadoEjemplarDto : DtoBase
{
    public int EjemplarId { get; set; }

    // Disponible, Reservado o FueraDeServicio. "Prestado" solo cambia con préstamos y devoluciones.
    public string EstadoNuevo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}