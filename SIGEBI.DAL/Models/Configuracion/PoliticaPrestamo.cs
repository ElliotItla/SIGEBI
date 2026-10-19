namespace SIGEBI.DAL.Models.Configuracion;

public class PoliticaPrestamo : AuditEntity
{
    public string TipoUsuario { get; set; } = string.Empty;
    public int MaxPrestamos { get; set; }
    public int DiasPrestamo { get; set; }
}
