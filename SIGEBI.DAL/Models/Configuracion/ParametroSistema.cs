namespace SIGEBI.DAL.Models.Configuracion;

public class ParametroSistema : AuditEntity
{
    public string Clave { get; set; } = string.Empty;
    public int Valor { get; set; }
}
