namespace SIGEBI.Web.Models.Catalogo;

public class CambiarEstadoEjemplarViewModel
{
    public int EjemplarId { get; set; }

    // Para volver a la página del recurso
    public int RecursoId { get; set; }

    public string EstadoNuevo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}