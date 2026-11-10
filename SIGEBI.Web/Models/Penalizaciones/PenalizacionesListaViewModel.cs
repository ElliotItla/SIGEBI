using SIGEBI.BL.Dtos.Penalizacion;

namespace SIGEBI.Web.Models.Penalizaciones;

// Una lista de penalizaciones: la usan "Mis penalizaciones" y la consulta por usuario
public class PenalizacionesListaViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string? Ayuda { get; set; }

    // Muestra solo las activas. Si es falso, también las ya resueltas.
    public bool SoloActivas { get; set; }

    // Para quien consulta sus propias penalizaciones: avisa si hoy le impiden pedir préstamos
    public bool AvisarBloqueo { get; set; }

    public List<PenalizacionDto> Penalizaciones { get; set; } = new();
}