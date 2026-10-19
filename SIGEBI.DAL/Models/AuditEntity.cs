namespace SIGEBI.DAL.Models;

// Columnas comunes de las tablas que registran quién y cuándo creó o modificó cada fila.
// Estatus en falso significa dado de baja (no se borra nada).
public abstract class AuditEntity
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? UsuarioCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int? UsuarioModificacion { get; set; }
    public bool Estatus { get; set; } = true;
}
