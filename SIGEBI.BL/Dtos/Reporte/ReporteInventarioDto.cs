namespace SIGEBI.BL.Dtos.Reporte;

public class InventarioCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public int Recursos { get; set; }
    public int Ejemplares { get; set; }
    public int Disponibles { get; set; }
}

public class InventarioRecursoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Ejemplares { get; set; }
    public int Disponibles { get; set; }
}

public class ReporteInventarioDto
{
    public int TotalRecursos { get; set; }
    public int TotalEjemplares { get; set; }
    public int Disponibles { get; set; }
    public int Prestados { get; set; }
    public int Reservados { get; set; }
    public int FueraDeServicio { get; set; }
    public List<InventarioCategoriaDto> PorCategoria { get; set; } = new();
    public List<InventarioRecursoDto> PorRecurso { get; set; } = new();
}