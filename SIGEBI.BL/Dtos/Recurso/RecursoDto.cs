namespace SIGEBI.BL.Dtos.Recurso;

public class RecursoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int TotalEjemplares { get; set; }
    public int Disponibles { get; set; }
    public List<EjemplarDto> Ejemplares { get; set; } = new();
}