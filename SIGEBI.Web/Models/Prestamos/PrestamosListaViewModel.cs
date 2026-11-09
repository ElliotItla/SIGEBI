using SIGEBI.BL.Dtos.Prestamo;

namespace SIGEBI.Web.Models.Prestamos;

// Una lista de préstamos: la usan "Mis préstamos" y las consultas por usuario y por recurso
public class PrestamosListaViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string? Ayuda { get; set; }

    // "Pendiente", "Activo", "Devuelto", "Rechazado" o vacío para todos
    public string? Estado { get; set; }

    public bool MostrarFiltroEstado { get; set; }
    public bool MostrarUsuario { get; set; }
    public bool MostrarRecurso { get; set; } = true;

    public List<PrestamoDto> Prestamos { get; set; } = new();
}