using SIGEBI.DAL.Models.Seguridad;

namespace SIGEBI.DAL.Models.Auditoria;

// Registro que solo se agrega: la base impide modificarlo o borrarlo.
public class RegistroAuditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string TipoAccion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string Resultado { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    public Usuario? Usuario { get; set; }
}
