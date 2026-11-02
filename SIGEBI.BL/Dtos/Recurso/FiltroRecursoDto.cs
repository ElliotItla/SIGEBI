using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Recurso;

public class FiltroRecursoDto : DtoBase
{
    public string? Titulo { get; set; }
    public string? Autor { get; set; }
    public string? Categoria { get; set; }
    public bool SoloDisponibles { get; set; }
}