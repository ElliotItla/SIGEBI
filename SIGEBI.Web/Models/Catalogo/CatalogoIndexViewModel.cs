using SIGEBI.BL.Dtos.Recurso;

namespace SIGEBI.Web.Models.Catalogo;

public class CatalogoIndexViewModel
{
    public string? Titulo { get; set; }
    public string? Autor { get; set; }
    public string? Categoria { get; set; }
    public bool SoloDisponibles { get; set; }

    public List<RecursoDto> Recursos { get; set; } = new();

    // El personal y el administrador registran recursos
    public bool PuedeAdministrar { get; set; }
}