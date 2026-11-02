using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Recurso;

public class GuardarRecursoDto : DtoBase
{
    // ISBN u otro identificador único del recurso
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadEjemplares { get; set; }
}