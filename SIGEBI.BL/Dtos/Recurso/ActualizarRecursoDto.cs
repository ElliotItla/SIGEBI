using SIGEBI.BL.Base;

namespace SIGEBI.BL.Dtos.Recurso;

// El código del recurso no se cambia
public class ActualizarRecursoDto : DtoBase
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
}