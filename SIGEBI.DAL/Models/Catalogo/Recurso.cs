namespace SIGEBI.DAL.Models.Catalogo;

public class Recurso : AuditEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    public ICollection<Ejemplar> Ejemplares { get; set; } = new List<Ejemplar>();
}
