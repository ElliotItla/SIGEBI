namespace SIGEBI.BL.Dtos.Usuario;

// Datos de un usuario que salen del sistema. Nunca incluyen la contraseña.
public class UsuarioDto
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
}