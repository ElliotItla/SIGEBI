using SIGEBI.BL.Dtos.Recurso;

namespace SIGEBI.Web.Models.Catalogo;

public class HistorialRecursoViewModel
{
    public int RecursoId { get; set; }

    // Puede venir vacío si el recurso ya se dio de baja
    public RecursoDto? Recurso { get; set; }

    public List<HistorialEjemplarDto> Cambios { get; set; } = new();
}