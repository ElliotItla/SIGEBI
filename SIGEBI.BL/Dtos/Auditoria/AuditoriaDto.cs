namespace SIGEBI.BL.Dtos.Auditoria;

public class AuditoriaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? Usuario { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string TipoAccion { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}