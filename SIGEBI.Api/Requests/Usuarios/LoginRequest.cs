using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Usuarios;

public class LoginRequest
{
    /// <example>admin@itla.edu.do</example>
    [Required(ErrorMessage = "Escribe tu correo.")]
    public string Correo { get; set; } = string.Empty;

    /// <example>Admin12345</example>
    [Required(ErrorMessage = "Escribe tu contraseña.")]
    public string Contrasena { get; set; } = string.Empty;
}