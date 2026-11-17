using System.ComponentModel.DataAnnotations;

namespace SIGEBI.Api.Requests.Usuarios;

// El identificador del usuario no se puede cambiar
public class ActualizarUsuarioRequest
{
    /// <example>Sofía</example>
    [Required(ErrorMessage = "Escribe el nombre.")]
    [StringLength(80, ErrorMessage = "El nombre no puede tener más de 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <example>Reyes Peña</example>
    [Required(ErrorMessage = "Escribe el apellido.")]
    [StringLength(80, ErrorMessage = "El apellido no puede tener más de 80 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    /// <example>sofia@itla.edu.do</example>
    [Required(ErrorMessage = "Escribe el correo.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Estudiante, Docente, PersonalBibliotecario, Administrador o Auditor.</summary>
    /// <example>Docente</example>
    [Required(ErrorMessage = "Elige el tipo de usuario.")]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Si se deja vacía, la contraseña no cambia.</summary>
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string? NuevaContrasena { get; set; }
}