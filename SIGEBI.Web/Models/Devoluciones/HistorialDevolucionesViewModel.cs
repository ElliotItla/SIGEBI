using SIGEBI.BL.Dtos.Devolucion;

namespace SIGEBI.Web.Models.Devoluciones;

public class HistorialDevolucionesViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string? Ayuda { get; set; }
    public bool MostrarUsuario { get; set; }
    public bool MostrarRecurso { get; set; } = true;

    public List<DevolucionDto> Devoluciones { get; set; } = new();
}